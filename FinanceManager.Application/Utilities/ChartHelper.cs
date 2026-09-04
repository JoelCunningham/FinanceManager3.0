namespace FinanceManager.Application.Utilities;

using FinanceManager.Application.Constants;
using FinanceManager.Application.DTOs;
using FinanceManager.Application.Enums;
using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Utilities;

//Deprecated 
public class ChartHelper(ITransactionRepository transactionRepository, IBudgetEntryRepository budgetEntryRepository)
{
    public static object CreateBarSeries(string name, string colour, decimal[] values, string? key) => new
    {
        name,
        type = "bar",
        stack = "total",
        color = colour,
        emphasis = new { focus = "series" },
        data = key is null
            ? [.. values.Select(v => (object)(double)v)]
            : values.Select(v => (object)new { value = (double)v, key }).ToArray()
    };

    public static object CreateLineSeries(string name, IEnumerable<decimal> data, int symbolSize) => new
    {
        name,
        type = "line",
        smooth = false,
        symbol = "circle",
        symbolSize,
        z = 20,
        lineStyle = new { width = 3 },
        data
    };

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

    public static (Dictionary<string, Guid> GroupIdByName, string? DrilldownGroupName, CategoryGroupSummary? DrilldownGroup) BuildGroupDrilldownMeta(Guid? drilldownGroupId, IEnumerable<CategoryGroupSummary> CategoryGroups)
    {
        var groupIdByName = CategoryGroups.ToDictionary(g => g.Name, g => g.Id, StringComparer.OrdinalIgnoreCase);
        var drilledGroup = drilldownGroupId is Guid id ? CategoryGroups.FirstOrDefault(g => g.Id == id) : null;
        return (groupIdByName, drilledGroup?.Name, drilledGroup);
    }

    public static List<T> ApplyGroupFilter<T>(IEnumerable<T> items, Guid? groupId, Func<T, Guid?> groupIdSelector)
    {
        return groupId is Guid id ? [.. items.Where(x => groupIdSelector(x) == id)] : [.. items];
    }

    public static bool IsTransactionInMode(TransactionSummary t, ChartMode mode)
    {
        return mode switch
        {
            ChartMode.Income => t.Amount > 0m,
            ChartMode.Expense => t.Amount < 0m,
            _ => true
        };
    }

    public static string GetCategoryOrGroupLabel(TransactionSummary t, bool isCategoryDrilldown)
    {
        if (isCategoryDrilldown)
        {
            var categoryName = t.Category?.Name;
            return string.IsNullOrWhiteSpace(categoryName) ? CategoryConstants.UncategorisedName : categoryName;
        }

        var groupName = t.Category?.GroupName;
        return string.IsNullOrWhiteSpace(groupName) ? CategoryConstants.UncategorisedName : groupName;
    }

    public static decimal GetChartSortAmount(TransactionSummary t, ChartMode mode)
    {
        return Math.Abs(GetChartStackAmount(t, mode));
    }

    public static decimal GetChartStackAmount(TransactionSummary t, ChartMode mode)
    {
        return mode == ChartMode.Expense ? Math.Abs(t.Amount) : t.Amount;
    }
}
