namespace FinanceManager.WebApp.Components.Pages;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Enums;
using FinanceManager.Domain.Constants;
using FinanceManager.Domain.Enums;
using FinanceManager.WebApp.Components.Base;
using FinanceManager.WebApp.Enums;
using FinanceManager.WebApp.Models;
using Microsoft.AspNetCore.Components;
using System.Globalization;

public partial class Statistics : PageBase
{
    [Parameter][SupplyParameterFromQuery] public string? Name { get; set; }
    [Parameter][SupplyParameterFromQuery] public string? Group { get; set; }

    public ChartModel Chart1 { get; set; } = default!;
    public ChartModel Chart2 { get; set; } = default!;

    public CategorySummary? SelectedCategory { get; set; }

    private List<CategoryGroupSummary> CategoryGroups { get; set; } = [];
    public List<CategorySummary> AvailableCategories { get; set; } = [];

    private const string UncategorisedLabel = "Uncategorised";

    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.Today);
    private static readonly DateOnly YearStart = new(Today.Year, 1, 1);

    protected override async Task OnInitializedAsync()
    {
        var initialRange = new ScopedRange(BudgetScope.Monthly, YearStart, DateConstants.MONTHS_IN_YEAR);
        var currentScope = (await UseCases.GetBudgetScopesAsync(initialRange)).GreatestScopeInRange;

        Chart1 = new(currentScope, YearStart, ReloadChart1Async, DateConstants.MONTHS_IN_YEAR);
        Chart2 = new(currentScope, Today, ReloadChart2Async);

        CategoryGroups = [.. (await UseCases.GetCategoryGroupsAsync()).Groups];
        AvailableCategories = [.. (await UseCases.GetCategoriesAsync()).Categories];

        await Chart1.RefreshAsync();
        await Chart2.RefreshAsync();
    }

    protected override void OnParametersSet()
    {
        if (ActiveTabId == Tabs.Category.ToString() && Name is not null && Group is not null)
        {
            SelectedCategory = AvailableCategories.FirstOrDefault(c =>
                string.Equals(c.Name, Name, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(c.GroupName, Group, StringComparison.OrdinalIgnoreCase)
            );
        }
    }

    private async Task ReloadChart1Async()
    {
        Chart1.Options = null;
        var drilldownGroupId = Chart1.SelectedGroupId;
        var (groupIdByName, drilldownGroupName, _) = BuildGroupDrilldownMeta(drilldownGroupId);

        var chartData = await UseCases.GetChart1DataAsync(Chart1.Range, drilldownGroupId, Chart1.Mode);

        var chartTransactions = ApplyGroupFilter(chartData.Transactions, drilldownGroupId, t => t.Category?.GroupId);

        BuildChart1(chartTransactions, chartData.BudgetSeries, chartData.IncomeBudgetSeries, chartData.ExpenseBudgetSeries, drilldownGroupId is not null, groupIdByName, drilldownGroupName);

        StateHasChanged();
    }

    private async Task ReloadChart2Async()
    {
        Chart2.Options = null;

        var correctScope = (await UseCases.GetBudgetScopesAsync(Chart2.Range)).GreatestScopeInRange;

        if (Chart2.Range.Scope != correctScope)
        {
            var anchorDate = Chart2.Range.EndDate;
            if (correctScope is BudgetScope.Weekly or BudgetScope.Fortnightly)
            {
                while (ISOWeek.GetYear(anchorDate.ToDateTime(TimeOnly.MinValue)) > anchorDate.Year)
                {
                    anchorDate = anchorDate.AddDays(-1);
                }
            }

            Chart2.Range = new ScopedRange(correctScope, anchorDate, Chart2.Range.Length);
        }

        var drilldownGroupId = Chart2.SelectedGroupId;
        var (groupIdByName, drilldownGroupName, _) = BuildGroupDrilldownMeta(drilldownGroupId);

        var chartData = await UseCases.GetChart2DataAsync(Chart2.Range, drilldownGroupId, Chart2.Mode);

        var chartTransactions = ApplyGroupFilter(chartData.Transactions, drilldownGroupId, t => t.Category?.GroupId);

        BuildChart2(chartTransactions, chartData.BudgetTotals, drilldownGroupId is not null, groupIdByName, drilldownGroupName);

        StateHasChanged();
    }

    private void BuildChart1(
        List<TransactionSummary> transactions,
        decimal[]? budgetSeriesData,
        decimal[]? budgetIncomeSeriesData,
        decimal[]? budgetExpenseSeriesData,
        bool isCategoryDrilldown,
        Dictionary<string, Guid> groupIdByName,
        string? drilldownGroupName)
    {
        if (transactions.Count == 0 && budgetSeriesData == null && budgetIncomeSeriesData == null && budgetExpenseSeriesData == null) return;

        var totals = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

        foreach (var transaction in transactions)
        {
            if (!Chart1.Range.Includes(DateOnly.FromDateTime(transaction.Date)))
            {
                continue;
            }
            if (!IsTransactionInMode(transaction, Chart1.Mode, false)) continue;

            var category = GetCategoryOrGroupLabel(transaction, isCategoryDrilldown);
            var sortAmount = GetChartSortAmount(transaction, Chart1.Mode);

            totals[category] = totals.TryGetValue(category, out var current) ? current + sortAmount : sortAmount;
        }

        var categoryNames = totals
            .OrderByDescending(kvp => kvp.Value)
            .Select(kvp => kvp.Key)
            .ToList();

        var amountsByCategory = new Dictionary<string, decimal[]>(StringComparer.OrdinalIgnoreCase);
        foreach (var name in categoryNames)
        {
            amountsByCategory[name] = new decimal[Chart1.Range.Length];
        }

        var months = Chart1.Range.Periods.ToList();
        foreach (var t in transactions)
        {
            if (!IsTransactionInMode(t, Chart1.Mode, excludeNet: false)) continue;

            var month = months.FirstOrDefault(m => m.Includes(DateOnly.FromDateTime(t.Date)));
            var monthIndex = months.IndexOf(month!);

            var amount = GetChartStackAmount(t, Chart1.Mode);
            var category = GetCategoryOrGroupLabel(t, isCategoryDrilldown);

            if (amountsByCategory.TryGetValue(category, out var arr))
            {
                arr[monthIndex] += amount;
            }
        }

        if (totals.Count == 0 && (budgetSeriesData == null || budgetSeriesData.All(v => v == 0m)) && (budgetIncomeSeriesData == null || budgetIncomeSeriesData.All(v => v == 0m)) && (budgetExpenseSeriesData == null || budgetExpenseSeriesData.All(v => v == 0m)))
        {
            return;
        }

        var series = new List<object>();

        foreach (var name in categoryNames)
        {
            var key = isCategoryDrilldown
                ? null
                : (groupIdByName.TryGetValue(name, out var id) ? id.ToString() : null);

            series.Add(CreateBarSeries(name, amountsByCategory[name], key));
        }

        var groupBudgetPrefix = isCategoryDrilldown ? (drilldownGroupName ?? "Group") : null;
        var budgetName = groupBudgetPrefix is null ? "Budget" : $"{groupBudgetPrefix} Budget";
        var incomeBudgetName = groupBudgetPrefix is null ? "Budget (Income)" : $"{groupBudgetPrefix} Budget";
        var expenseBudgetName = groupBudgetPrefix is null ? "Budget (Expense)" : $"{groupBudgetPrefix} Budget";

        if (Chart1.Mode == TransactionsGraphMode.Net)
        {
            if (budgetIncomeSeriesData is not null && budgetIncomeSeriesData.Length == Chart1.Range.Length)
            {
                series.Add(CreateLineSeries(incomeBudgetName, budgetIncomeSeriesData, 7));
            }

            if (budgetExpenseSeriesData is not null && budgetExpenseSeriesData.Length == Chart1.Range.Length)
            {
                series.Add(CreateLineSeries(expenseBudgetName, budgetExpenseSeriesData, 7));
            }
        }
        else
        {
            if (budgetSeriesData is not null && budgetSeriesData.Length == Chart1.Range.Length)
            {
                series.Add(CreateLineSeries(budgetName, budgetSeriesData, 8));
            }
        }

        var xAxisLabels = Chart1.Range.Periods.Select(m => m.StartDate.ToString("yyyy-MM")).ToArray();

        Chart1.Title = (Chart1.Mode, isCategoryDrilldown) switch
        {
            (TransactionsGraphMode.Expense, false) => $"Expenses by group between {Chart1.Range.StartDate:MMM yyyy} and {Chart1.Range.EndDate:MMM yyyy}",
            (TransactionsGraphMode.Income, false) => $"Income by group between {Chart1.Range.StartDate:MMM yyyy} and {Chart1.Range.EndDate:MMM yyyy}",
            (TransactionsGraphMode.Net, false) => $"Net by group between {Chart1.Range.StartDate:MMM yyyy} and {Chart1.Range.EndDate:MMM yyyy}",
            (_, true) => $"{drilldownGroupName ?? "Group"} by category",
            _ => "Transactions"
        };
        Chart1.Options = new
        {
            tooltip = new { trigger = "axis" },
            legend = new { type = "scroll" },
            grid = new { left = "3%", right = "4%", bottom = "3%", containLabel = true },
            xAxis = new { type = "category", data = xAxisLabels },
            yAxis = new { type = "value" },
            series
        };
    }

    private void BuildChart2(
        List<TransactionSummary> transactions,
        IReadOnlyDictionary<string, decimal> budgetTotals,
        bool isCategoryDrilldown,
        Dictionary<string, Guid> groupIdByName,
        string? drilldownGroupName)
    {
        var actualTotals = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

        foreach (var t in transactions)
        {
            if (!IsTransactionInMode(t, Chart2.Mode, excludeNet: true)) continue;

            var name = GetCategoryOrGroupLabel(t, isCategoryDrilldown);
            var amount = GetChartSortAmount(t, Chart2.Mode);

            actualTotals[name] = actualTotals.TryGetValue(name, out var current)
                ? current + amount
                : amount;
        }

        var rows = actualTotals.Keys
            .Union(budgetTotals.Keys, StringComparer.OrdinalIgnoreCase)
            .Select(name => new
            {
                Name = name,
                Actual = actualTotals.GetValueOrDefault(name),
                Budget = Math.Round(budgetTotals.GetValueOrDefault(name), 2)
            })
            .Where(x => x.Actual != 0m || x.Budget != 0m)
            .OrderByDescending(x => x.Actual)
            .ThenByDescending(x => x.Budget)
            .ToList();

        if (rows.Count == 0) return;

        var labels = rows.Select(r => r.Name).ToArray();

        var drilldownKeys = rows
            .Select(r =>
            {
                if (isCategoryDrilldown) return null;
                return groupIdByName.TryGetValue(r.Name, out var id) ? id.ToString() : null;
            })
            .ToArray();

        var isIncome = Chart2.Mode == TransactionsGraphMode.Income;

        var baseValues = rows.Select(r => (double)Math.Min(r.Actual, r.Budget)).ToArray();
        var remainingValues = rows.Select(r => (double)Math.Max(r.Budget - r.Actual, 0m)).ToArray();
        var overValues = rows.Select(r => (double)Math.Max(r.Actual - r.Budget, 0m)).ToArray();

        object[] BuildSeriesData(double[] values) => [.. values.Select((v, i) => new { value = v, key = drilldownKeys[i] })];

        var modeText = Chart2.Mode == TransactionsGraphMode.Income ? "incomes" : "expenses";
        var levelText = drilldownGroupName is null ? "All" : drilldownGroupName;

        var baseLabel = isIncome ? "Earned within budget" : "Spent within budget";
        var remainingLabel = isIncome ? "Remaining budget" : "Remaining budget";
        var overLabel = isIncome ? "Above budget" : "Over budget";

        var baseColor = "#009de0";
        var remainingColor = isIncome ? "#dc3545" : "#198754";
        var overColor = isIncome ? "#198754" : "#dc3545";

        Chart2.Title = $"{levelText} {modeText} vs budget for {Chart2.Range.StartDate:MMM yyyy}";
        Chart2.Options = new
        {
            tooltip = new { trigger = "axis", axisPointer = new { type = "shadow" } },
            legend = new { type = "scroll" },
            grid = new { left = "3%", right = "4%", bottom = "10%", containLabel = true },
            xAxis = new
            {
                type = "category",
                data = labels,
                axisLabel = new
                {
                    interval = 0,
                    rotate = rows.Count > 6 ? 35 : 0
                }
            },
            yAxis = new { type = "value" },
            series = new object[]
            {
                new
                {
                    name = baseLabel,
                    type = "bar",
                    stack = "total",
                    emphasis = new { focus = "series" },
                    itemStyle = new { color = baseColor },
                    data = BuildSeriesData(baseValues)
                },
                new
                {
                    name = remainingLabel,
                    type = "bar",
                    stack = "total",
                    emphasis = new { focus = "series" },
                    itemStyle = new { color = remainingColor },
                    data = BuildSeriesData(remainingValues)
                },
                new
                {
                    name = overLabel,
                    type = "bar",
                    stack = "total",
                    emphasis = new { focus = "series" },
                    itemStyle = new { color = overColor },
                    data = BuildSeriesData(overValues)
                }
            }
        };
    }

    private static List<T> ApplyGroupFilter<T>(IEnumerable<T> items, Guid? groupId, Func<T, Guid?> groupIdSelector)
    {
        return groupId is Guid id ? [.. items.Where(x => groupIdSelector(x) == id)] : [.. items];
    }

    private (Dictionary<string, Guid> GroupIdByName, string? DrilldownGroupName, CategoryGroupSummary? DrilldownGroup) BuildGroupDrilldownMeta(Guid? drilldownGroupId)
    {
        var groupIdByName = CategoryGroups.ToDictionary(g => g.Name, g => g.Id, StringComparer.OrdinalIgnoreCase);
        var drilledGroup = drilldownGroupId is Guid id ? CategoryGroups.FirstOrDefault(g => g.Id == id) : null;
        return (groupIdByName, drilledGroup?.Name, drilledGroup);
    }

    private static bool IsTransactionInMode(TransactionSummary t, TransactionsGraphMode mode, bool excludeNet)
    {
        if (excludeNet && mode == TransactionsGraphMode.Net) return false;

        return mode switch
        {
            TransactionsGraphMode.Income => t.Amount > 0m,
            TransactionsGraphMode.Expense => t.Amount < 0m,
            _ => true
        };
    }

    private static decimal GetChartStackAmount(TransactionSummary t, TransactionsGraphMode mode)
    {
        return mode == TransactionsGraphMode.Expense ? Math.Abs(t.Amount) : t.Amount;
    }

    private static decimal GetChartSortAmount(TransactionSummary t, TransactionsGraphMode mode)
    {
        return Math.Abs(GetChartStackAmount(t, mode));
    }

    private static string GetCategoryOrGroupLabel(TransactionSummary t, bool isCategoryDrilldown)
    {
        if (isCategoryDrilldown)
        {
            var categoryName = t.Category?.Name;
            return string.IsNullOrWhiteSpace(categoryName) ? UncategorisedLabel : categoryName;
        }

        var groupName = t.Category?.GroupName;
        return string.IsNullOrWhiteSpace(groupName) ? UncategorisedLabel : groupName;
    }

    private static object CreateBarSeries(string name, decimal[] values, string? key) => new
    {
        name,
        type = "bar",
        stack = "total",
        emphasis = new { focus = "series" },
        data = key is null
            ? values.Select(v => (object)(double)v).ToArray()
            : values.Select(v => (object)new { value = (double)v, key }).ToArray()
    };

    private static object CreateLineSeries(string name, decimal[] data, int symbolSize) => new
    {
        name,
        type = "line",
        smooth = true,
        symbol = "circle",
        symbolSize,
        z = 20,
        lineStyle = new { width = 3 },
        data
    };
}