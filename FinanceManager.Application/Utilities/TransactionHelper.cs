namespace FinanceManager.Application.Utilities;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Utilities;

public class TransactionHelper(ITransactionRepository transactionRepository, IBudgetEntryRepository budgetEntryRepository)
{
    public async Task<List<TransactionSummary>> GetTransactionsForRange(FilterQuery query)
    {
        var results = new List<TransactionSummary>();
        var page = 1;

        while (true)
        {
            query.Page = page;
            var pagedTransactions = await transactionRepository.GetPagedAsync(query);
            var pageResult = new PagedResult<TransactionSummary>
            {
                Items = [.. pagedTransactions.Items.Select(TransactionSummary.FromTransaction)],
                TotalItems = pagedTransactions.TotalItems,
                CurrentPage = pagedTransactions.CurrentPage,
                PageSize = pagedTransactions.PageSize
            };

            if (pageResult.Items.Count == 0) break;
            results.AddRange(pageResult.Items);

            if (results.Count >= pageResult.TotalItems) break;
            page++;
        }

        return results;
    }

    public async Task<decimal[]> GetBudgetPerMonthForCategories(ScopedPeriod period, List<CategorySummary> categories, bool asExpense)
    {
        if (categories.Count == 0) return [];

        var budgetsPerMonth = await GetBudgetPerMonth(period.StartDate, period.EndDate, categories);
        return [.. budgetsPerMonth.Select(b => !asExpense ? b.Value : -b.Value)];
    }

    public async Task<Dictionary<string, decimal>> GetBudgetPerLabel(DateOnly start, DateOnly end, List<CategorySummary> categories, bool isCategoryDrilldown)
    {
        var budgetsInPeriod = await budgetEntryRepository.GetByRangeAsync(start, end, categories.Select(c => c.Id).ToHashSet());

        var totals = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

        foreach (var budget in budgetsInPeriod)
        {
            foreach (var day in BudgetEntryPeriodHelper.GetOverlappingDays(budget, start, end))
            {
                var label = isCategoryDrilldown ? budget.Category.Name : budget.Category.Group.Name;
                if (string.IsNullOrWhiteSpace(label))
                {
                    continue;
                }

                totals[label] = totals.TryGetValue(label, out var current)
                    ? current + Math.Abs(day.DailyAmount)
                    : Math.Abs(day.DailyAmount);
            }
        }

        return totals;
    }

    private async Task<IDictionary<DateOnly, decimal>> GetBudgetPerMonth(DateOnly startDate, DateOnly endDate, IEnumerable<CategorySummary> categories)
    {
        var budgetsPerDay = await GetBudgetPerDay(startDate, endDate, categories);
        var budgetsPerMonth = budgetsPerDay
            .GroupBy(d => new { d.Key.Year, d.Key.Month })
            .ToDictionary(
                g => new DateOnly(g.Key.Year, g.Key.Month, 1),
                g => g.Sum(x => x.Value)
            );

        return budgetsPerMonth;
    }

    private async Task<IDictionary<DateOnly, decimal>> GetBudgetPerDay(DateOnly startDate, DateOnly endDate, IEnumerable<CategorySummary> categories)
    {
        var budgetsInPeriod = await budgetEntryRepository.GetByRangeAsync(startDate, endDate, categories.Select(c => c.Id).ToHashSet());

        var amountPerDay = new Dictionary<DateOnly, decimal>();

        var currentDate = startDate;
        while (currentDate <= endDate)
        {
            amountPerDay[currentDate] = 0m;
            currentDate = currentDate.AddDays(1);
        }

        foreach (var budget in budgetsInPeriod)
        {
            foreach (var day in BudgetEntryPeriodHelper.GetOverlappingDays(budget, startDate, endDate))
            {
                amountPerDay[day.Date] += day.DailyAmount;
            }
        }

        return amountPerDay;
    }

}
