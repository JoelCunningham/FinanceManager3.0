namespace FinanceManager.Application.Utilities;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Enums;
using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Utilities;

public class ChartHelper(ITransactionRepository transactionRepository, IBudgetEntryRepository budgetEntryRepository)
{
    public async Task<Dictionary<Guid, IEnumerable<decimal>>> GetTransactions(IEnumerable<CategorySummary> categories, IEnumerable<ScopedPeriod> periods, bool isExpense = false, bool strict = false, bool byGroup = false)
    {
        var query = new FilterQuery
        {
            FilterDateFrom = periods.Min(p => p.StartDate).ToDateTime(TimeOnly.MinValue),
            FilterDateTo = periods.Max(p => p.EndDate).ToDateTime(TimeOnly.MaxValue),
            FilterStatus = ReviewStatus.Reviewed,
            FilterCategories = [.. categories],
            FilterAmountMin = strict && !isExpense ? 0 : null,
            FilterAmountMax = strict && isExpense ? 0 : null
        };
        var transactions = await transactionRepository.GetTransactionsAsync(query);

        var transactionGroups = transactions
            .GroupBy(t => byGroup ? t.Category!.GroupId : t.Category!.Id)
            .ToDictionary(g => g.Key, g => g.ToList());

        return transactionGroups.ToDictionary(group => group.Key, group => periods
            .Select(p => group.Value
                .Where(t => DateOnly.FromDateTime(t.Date) >= p.StartDate && DateOnly.FromDateTime(t.Date) <= p.EndDate)
                .Sum(t => isExpense ? -t.TotalAmount : t.TotalAmount)
        ));
    }

    public async Task<Dictionary<Guid, IEnumerable<decimal>>> GetBudgets(IEnumerable<CategorySummary> categories, IEnumerable<ScopedPeriod> periods, bool isExpense = false, bool byGroup = false, bool isNet = false)
    {
        var categoryLookup = categories.ToDictionary(c => c.Id);

        var budgets = await budgetEntryRepository.GetByRangeAsync(periods.Min(p => p.StartDate), periods.Max(p => p.EndDate), categoryLookup.Keys.ToHashSet());
        var budgetGroups = budgets
            .GroupBy(b => byGroup ? categoryLookup[b.CategoryId].GroupId : b.CategoryId)
            .ToDictionary(g => g.Key, g => g.ToList());

        return budgetGroups.ToDictionary(group => group.Key, group => periods.Select(p => group.Value
            .SelectMany(b => BudgetYearHelper.GetOverlappingDays(b, p.StartDate, p.EndDate))
            .Sum(d => 
                isExpense ? -d.DailyAmount : 
                isNet ? 
                    d.IsExpense ? 
                    -d.DailyAmount : d.DailyAmount :
                d.DailyAmount))
        );
    }

    public static IEnumerable<decimal> FlattenSerieses(Dictionary<Guid, IEnumerable<decimal>> serieses)
    {
        return serieses.Values
            .SelectMany((values, _) => values.Select((value, index) => (value, index)))
            .GroupBy(x => x.index).OrderBy(g => g.Key)
            .Select(g => g.Sum(x => x.value));
    }

    public static IEnumerable<decimal> CumulativeSeries(IEnumerable<decimal> series)
    {
        decimal cumulative = 0;
        return series.Select(value => cumulative += value);
    }
}
