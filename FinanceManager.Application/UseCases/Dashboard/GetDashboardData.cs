namespace FinanceManager.Application.UseCases.Dashboard;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Enums;
using FinanceManager.Application.Interfaces;
using FinanceManager.Application.UseCases.Categories;
using FinanceManager.Domain.Enums;
using FinanceManager.Domain.Utilities;

public sealed record GetDashboardDataResult(
    IReadOnlyList<BudgetCategoryUsage> BudgetCategoryUsages,
    decimal SpentThisMonth,
    decimal BudgetedThisMonth,
    int OverBudgetCategories,
    int UnassignedTransactions,
    decimal IncomeShare,
    int DaysSinceLastImport,
    string? TopOverBudgetCategory,
    decimal TopOverBudgetAmount,
    decimal TopOverBudgetProportion,
    IReadOnlyList<ScopedPeriod> AvailablePeriods
) : UseCaseResult;

public sealed class GetDashboardData(
    ITransactionRepository transactionRepository,
    IBudgetEntryRepository budgetEntryRepository,
    IBankRecordRepository bankRecordRepository,
    IBudgetYearRepository budgetYearRepository,
    GetCategories getCategoryList)
{
    public async Task<GetDashboardDataResult> ExecuteAsync(ScopedPeriod? period = null)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var availablePeriods = (await GetAllPeriods(12)).ToList();

        period ??= availablePeriods.FirstOrDefault(p => p.Includes(today)) ?? availablePeriods.FirstOrDefault() ?? new ScopedPeriod(BudgetScope.Monthly, today, 0);

        var monthRange = new ScopedRange(period.Scope, period.StartDate);

        var categories = (await getCategoryList.ExecuteAsync()).Categories;
        var expenseCategories = categories.Where(category => !category.IsIncome).ToList();

        var reviewedTransactions = await GetReviewedTransactionsAsync(monthRange);
        var expenseTransactions = reviewedTransactions.Where(transaction => transaction.Amount < 0m).ToList();
        var incomeTransactions = reviewedTransactions.Where(transaction => transaction.Amount > 0m).ToList();

        var budgetEntries = await budgetEntryRepository.GetByRangeAsync(monthRange.StartDate, monthRange.EndDate, expenseCategories.Select(category => category.Id));
        var budgetByCategory = expenseCategories.ToDictionary(category => category.Id, _ => 0m);

        foreach (var budgetEntry in budgetEntries)
        {
            var budgetTotal = BudgetYearHelper.GetOverlappingDays(budgetEntry, monthRange.StartDate, monthRange.EndDate).Sum(day => Math.Abs(day.DailyAmount));

            budgetByCategory[budgetEntry.CategoryId] = budgetByCategory.TryGetValue(budgetEntry.CategoryId, out var current)
                ? current + budgetTotal
                : budgetTotal;
        }

        var spentByCategory = expenseCategories.ToDictionary(category => category.Id, _ => 0m);
        foreach (var transaction in expenseTransactions)
        {
            if (transaction.Category is null) continue;

            spentByCategory[transaction.Category.Id] = spentByCategory.TryGetValue(transaction.Category.Id, out var current)
                ? current + Math.Abs(transaction.Amount)
                : Math.Abs(transaction.Amount);
        }

        var budgetCategoryUsages = expenseCategories
            .Select(category =>
            {
                var budgetedAmount = budgetByCategory.GetValueOrDefault(category.Id);
                var spentAmount = spentByCategory.GetValueOrDefault(category.Id);

                return new BudgetCategoryUsage
                {
                    Summary = category,
                    BudgetedAmount = budgetedAmount,
                    Amount = spentAmount,
                    Proportion = budgetedAmount <= 0m ? (spentAmount > 0m ? 1m : 0m) : spentAmount / budgetedAmount,
                };
            })
            .Where(usage => usage.Amount > 0m || usage.Proportion > 0m)
            .OrderByDescending(usage => usage.Amount)
            .ToList();

        var spentThisMonth = expenseTransactions.Sum(transaction => Math.Abs(transaction.Amount));
        var budgetedThisMonth = budgetEntries
            .SelectMany(entry => BudgetYearHelper.GetOverlappingDays(entry, monthRange.StartDate, monthRange.EndDate))
            .Sum(day => Math.Abs(day.DailyAmount));
        var overBudgetCategories = budgetCategoryUsages.Count(usage => usage.Proportion > 1m);

        var totalTransactedValue = reviewedTransactions.Sum(transaction => Math.Abs(transaction.Amount));
        var incomeShare = totalTransactedValue == 0m ? 0m : incomeTransactions.Sum(transaction => Math.Abs(transaction.Amount)) / totalTransactedValue;

        var topOverBudget = budgetCategoryUsages
            .Where(usage => usage.Proportion > 1m)
            .OrderByDescending(usage => usage.Proportion)
            .FirstOrDefault();

        var unassignedTransactions = await GetUnassignedTransactionCountAsync();
        var daysSinceLastImport = await GetDaysSinceLastImportAsync();

        return new GetDashboardDataResult(
            budgetCategoryUsages,
            spentThisMonth,
            budgetedThisMonth,
            overBudgetCategories,
            unassignedTransactions,
            incomeShare,
            daysSinceLastImport,
            topOverBudget?.Summary.Name,
            topOverBudget is null ? 0m : Math.Max(0m, topOverBudget.Amount - topOverBudget.BudgetedAmount),
            topOverBudget?.Proportion ?? 0m,
            availablePeriods
        );
    }

    private async Task<List<TransactionSummary>> GetReviewedTransactionsAsync(ScopedRange monthRange)
    {
        var query = new FilterQuery
        {
            FilterDateFrom = monthRange.StartDate.ToDateTime(TimeOnly.MinValue),
            FilterDateTo = monthRange.EndDate.ToDateTime(TimeOnly.MaxValue),
            FilterStatus = ReviewStatus.Reviewed,
        };

        return [.. (await transactionRepository.GetTransactionsAsync(query)).Select(TransactionSummary.FromTransaction)];
    }

    private async Task<int> GetUnassignedTransactionCountAsync()
    {
        var query = new FilterQuery
        {
            PageSize = 1,
            FilterStatus = ReviewStatus.Unreviewed,
        };

        var paged = await transactionRepository.GetPagedTransactionsAsync(query);
        return paged.TotalItems;
    }

    private async Task<int> GetDaysSinceLastImportAsync()
    {
        var latestImportDate = await bankRecordRepository.GetLatestDateAsync();
        if (latestImportDate is null) return 0;

        return Math.Max(0, DateOnly.FromDateTime(DateTime.Today).DayNumber - DateOnly.FromDateTime(latestImportDate.Value).DayNumber);
    }

    private async Task<IEnumerable<ScopedPeriod>> GetAllPeriods(int limit)
    {
        var periods = new List<ScopedPeriod>();
        var currentYear = DateTime.Today.Year;

        while (periods.Count < limit)
        {
            var currentScope = (await budgetYearRepository.GetByYearAsync(currentYear))?.Scope ?? BudgetScope.Monthly;
            var entryPeriods = BudgetYearHelper.GetPeriods(currentYear, currentScope);

            if (currentYear == DateTime.Today.Year)
            {
                entryPeriods = [.. entryPeriods.Where(period => period.StartDate <= DateOnly.FromDateTime(DateTime.Today))];
            }

            periods.AddRange(entryPeriods.Select(period => new ScopedPeriod
            {
                Scope = currentScope,
                StartDate = period.StartDate,
                EndDate = period.EndDate,
            }));

            currentYear--;
        }

        periods = periods.OrderByDescending(period => period.StartDate).ThenByDescending(period => period.EndDate).ToList();

        if (periods.Count > limit)
        {
            periods.RemoveRange(limit, periods.Count - limit);
        }

        if (periods.Count == 0)
        {
            periods.Add(new ScopedPeriod
            {
                Scope = BudgetScope.Monthly,
                StartDate = DateOnly.FromDateTime(DateTime.Today).AddMonths(-1),
                EndDate = DateOnly.FromDateTime(DateTime.Today),
            });
        }

        return periods;
    }
}