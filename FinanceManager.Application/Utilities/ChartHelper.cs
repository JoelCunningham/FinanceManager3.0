namespace FinanceManager.Application.Utilities;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Enums;
using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Utilities;

//Deprecated 
public class ChartHelper(ITransactionRepository transactionRepository, IBudgetEntryRepository budgetEntryRepository)
{
    public async Task<List<decimal>> GetTransactionsPerPeriod(CategorySummary category, IEnumerable<ScopedPeriod> periods)
    {
        var query = new FilterQuery
        {
            FilterDateFrom = periods.Min(p => p.StartDate).ToDateTime(TimeOnly.MinValue),
            FilterDateTo = periods.Max(p => p.EndDate).ToDateTime(TimeOnly.MaxValue),
            FilterStatus = ReviewStatus.Reviewed,
            FilterCategory = category
        };

        var transactions = await transactionRepository.GetTransactionsAsync(query);

        var transactionsPerPeriod = periods.Select(p => transactions
            .Where(t => DateOnly.FromDateTime(t.Date) >= p.StartDate && DateOnly.FromDateTime(t.Date) <= p.EndDate)
            .Sum(t => t.Amount)
        );

        return [.. transactionsPerPeriod];
    }

    public async Task<List<decimal>> GetBudgetsPerPeriod(CategorySummary category, IEnumerable<ScopedPeriod> periods, bool isExpense = false)
    {
        var bugets = await budgetEntryRepository.GetByRangeAsync(periods.Min(p => p.StartDate), periods.Max(p => p.EndDate), new HashSet<Guid> { category.Id });

        var budgetsPerPeriod = periods.ToDictionary(
            p => p,
            p => bugets
                .SelectMany(b => BudgetYearHelper.GetOverlappingDays(b, p.StartDate, p.EndDate))
                .Sum(d => isExpense ? -d.DailyAmount : d.DailyAmount)
        );

        return [.. budgetsPerPeriod.Values];
    }

    public async Task<Dictionary<string, decimal>> GetBudgetPerLabel(DateOnly start, DateOnly end, List<CategorySummary> categories, bool isCategoryDrilldown)
    {
        var budgetsInRange = await budgetEntryRepository.GetByRangeAsync(start, end, categories.Select(c => c.Id).ToHashSet());

        var totals = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

        foreach (var budget in budgetsInRange)
        {
            foreach (var day in BudgetYearHelper.GetOverlappingDays(budget, start, end))
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

}
