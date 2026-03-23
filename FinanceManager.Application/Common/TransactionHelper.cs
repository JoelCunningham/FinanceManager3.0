namespace FinanceManager.Application.Common;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Entities;

public class TransactionHelper(ITransactionRepository transactionRepository, IBudgetEntryRepository budgetEntryRepository)
{
    public async Task<List<TransactionSummary>> GetTransactionsForRange(FilterQuery query)
    {
        var results = new List<TransactionSummary>();
        var page = 1;

        while (true)
        {
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

    public async Task<decimal[]> GetBudgetPerMonthForCategories(ScopedPeriod period, List<Category> categories, bool asExpense)
    {
        if (categories.Count == 0) return [];

        var budgetsPerDay = await GetBudgetPerDay(period.StartDate, period.EndDate, categories);
        var budgetsPerMonth = await GetBudgetPerMonth(period.StartDate, period.EndDate, categories);
        return [.. budgetsPerMonth.Select(b => !asExpense ? b.Value : -b.Value)];
    }

    public async Task<Dictionary<string, decimal>> GetBudgetPerLabel(DateOnly start, DateOnly end, List<Category> categories, bool isCategoryDrilldown)
    {
        var budgetsInPeriod = await budgetEntryRepository.GetByRangeAsync(start, end, categories);

        var totals = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

        var currentDate = start;
        while (currentDate <= end)
        {
            var budgetsInDay = budgetsInPeriod.Where(b => currentDate >= b.StartDate && currentDate <= b.EndDate);

            foreach (var budget in budgetsInDay)
            {
                var label = isCategoryDrilldown ? budget.Category.Name : budget.Category.Group.Name;
                if (string.IsNullOrWhiteSpace(label)) continue;

                totals[label] = totals.TryGetValue(label, out var current)
                    ? current + Math.Abs(budget.DailyAmount)
                    : Math.Abs(budget.DailyAmount);
            }

            currentDate = currentDate.AddDays(1);
        }

        return totals;
    }

    private async Task<IDictionary<DateOnly, decimal>> GetBudgetPerMonth(DateOnly startDate, DateOnly endDate, IEnumerable<Category> categories)
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

    private async Task<IDictionary<DateOnly, decimal>> GetBudgetPerDay(DateOnly startDate, DateOnly endDate, IEnumerable<Category> categories)
    {
        var budgetsInPeriod = await budgetEntryRepository.GetByRangeAsync(startDate, endDate, categories);

        var currentDate = startDate;
        var amountPerDay = new Dictionary<DateOnly, decimal>();

        while (currentDate <= endDate)
        {
            var budgetsInDay = budgetsInPeriod.Where(b => currentDate >= b.StartDate && currentDate <= b.EndDate);
            amountPerDay[currentDate] = budgetsInDay.Sum(b => b.DailyAmount);
            currentDate = currentDate.AddDays(1);
        }

        return amountPerDay;
    }
}
