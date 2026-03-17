namespace FinanceManager.WebApp.Components.Pages;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Services;
using FinanceManager.Domain.Entities;
using FinanceManager.Domain.Enums;
using FinanceManager.WebApp.Enums;
using FinanceManager.WebApp.Models;
using Microsoft.AspNetCore.Components;

public partial class Transactions : ComponentBase
{
    [Inject] public BudgetService BudgetService { get; set; } = default!;
    [Inject] public CategoryService CategoryService { get; set; } = default!;
    [Inject] public TransactionService TransactionService { get; set; } = default!;

    public List<Category> Categories { get; set; } = [];
    private IReadOnlyList<CategoryGroup> CategoryGroups => [.. Categories.Select(c => c.Group).DistinctBy(g => g.Id).OrderBy(g => g.Name)];

    public ChartModel Chart1 { get; set; } = default!;
    public ChartModel Chart2 { get; set; } = default!;

    private bool Chart2HasLargerScopedBudgets { get; set; }
    
    private const string UncategorisedLabel = "Uncategorised";

    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.Today);

    protected override async Task OnInitializedAsync()
    {
        var currentScope = await BudgetService.GetCurrentScope() ?? Scope.Monthly;
        var currentPeriod = new ScopedPeriod(currentScope, Today);

        Chart1 = new(currentScope, Today, ReloadChart1Async);
        Chart2 = new(currentScope, Today, ReloadChart2Async);

        Categories = [.. await CategoryService.GetCategoriesAsync()];
        Chart2HasLargerScopedBudgets = (await GetGreatestScopeInPeriod(Chart2.Period)) > Chart2.Period.Scope;

        await Chart1.RefreshAsync();
        await Chart2.RefreshAsync();
    }

    private async Task<Scope> GetGreatestScopeInPeriod(ScopedPeriod period)
    {
        return await BudgetService.GetGreatestScopeInPeriod(period);
    }

    private async Task ReloadChart1Async()
    {
        Chart1.Options = null;

        var query = new FilterQuery
        {
            FilterDateFrom = Chart1.Period.StartDate.ToDateTime(TimeOnly.MinValue),
            FilterDateTo = Chart1.Period.EndDate.ToDateTime(TimeOnly.MinValue),
            FilterStatus = ReviewStatus.Reviewed
        };

        var drilldownGroupId = Chart1.SelectedGroupId;
        var (groupIdByName, drilldownGroupName, drilledGroup) = BuildGroupDrilldownMeta(drilldownGroupId);

        var incomeCategories = Categories.Where(c => c.Group.IsIncome).ToList();
        var expenseCategories = Categories.Where(c => !c.Group.IsIncome).ToList();

        var transactions = await TransactionService.GetAllAsync<TransactionSummary>(query);
        var chartTransactions = ApplyGroupFilter(transactions, drilldownGroupId, t => t.Category?.GroupId);

        incomeCategories = ApplyGroupFilter(incomeCategories, drilldownGroupId, c => c.GroupId);
        expenseCategories = ApplyGroupFilter(expenseCategories, drilldownGroupId, c => c.GroupId);

        double[]? budgetSeriesData = null;
        double[]? budgetIncomeSeriesData = null;
        double[]? budgetExpenseSeriesData = null;

        if (Chart1.Mode == TransactionsGraphMode.Net)
        {
            if (drilldownGroupId is null)
            {
                budgetIncomeSeriesData = await BuildChart1BudgetSeriesAsync(Chart1.Period, incomeCategories, false);
                budgetExpenseSeriesData = await BuildChart1BudgetSeriesAsync(Chart1.Period, expenseCategories, true);
            }
            else
            {
                if (drilledGroup?.IsIncome == true)
                {
                    budgetIncomeSeriesData = await BuildChart1BudgetSeriesAsync(Chart1.Period, incomeCategories, false);
                }
                else if (drilledGroup?.IsIncome == false)
                {
                    budgetExpenseSeriesData = await BuildChart1BudgetSeriesAsync(Chart1.Period, expenseCategories, true);
                }
                else
                {
                    budgetIncomeSeriesData = await BuildChart1BudgetSeriesAsync(Chart1.Period, incomeCategories, false);
                    budgetExpenseSeriesData = await BuildChart1BudgetSeriesAsync(Chart1.Period, expenseCategories, true);
                }
            }
        }
        else
        {
            var relevantCategories = Chart1.Mode == TransactionsGraphMode.Income ? incomeCategories : expenseCategories;
            budgetSeriesData = await BuildChart1BudgetSeriesAsync(Chart1.Period, relevantCategories, false);
        }

        Chart1.Options = BuildChart1(chartTransactions, Chart1.Period, Chart1.Mode, budgetSeriesData, budgetIncomeSeriesData, budgetExpenseSeriesData, drilldownGroupId is not null, groupIdByName, drilldownGroupName);

        StateHasChanged();
    }

    private async Task ReloadChart2Async()
    {
        Chart2.Options = null;

        var query = new FilterQuery
        {
            FilterDateFrom = Chart2.Period.StartDate.ToDateTime(TimeOnly.MinValue),
            FilterDateTo = Chart2.Period.EndDate.ToDateTime(TimeOnly.MinValue),
            FilterStatus = ReviewStatus.Reviewed
        };

        var drilldownGroupId = Chart2.SelectedGroupId;
        var (groupIdByName, drilldownGroupName, _) = BuildGroupDrilldownMeta(drilldownGroupId);

        var relevantCategories = Chart2.Mode == TransactionsGraphMode.Income
            ? Categories.Where(c => c.Group.IsIncome).ToList()
            : Categories.Where(c => !c.Group.IsIncome).ToList();

        var transactions = await TransactionService.GetAllAsync<TransactionSummary>(query);

        var chartTransactions = ApplyGroupFilter(transactions, drilldownGroupId, t => t.Category?.GroupId);
        var filteredCategories = ApplyGroupFilter(relevantCategories, drilldownGroupId, c => c.GroupId);

        var budgetEntries = (await BudgetService.GetBudget(Chart2.Period, filteredCategories)).ToList();

        Chart2.Options = BuildChart2(chartTransactions, budgetEntries, Chart2.Mode, drilldownGroupId is not null, groupIdByName, drilldownGroupName);

        StateHasChanged();
    }

    private static object? BuildChart1(
        List<TransactionSummary> transactions,
        ScopedPeriod period,
        TransactionsGraphMode mode,
        double[]? budgetSeriesData,
        double[]? budgetIncomeSeriesData,
        double[]? budgetExpenseSeriesData,
        bool isCategoryDrilldown,
        Dictionary<string, Guid> groupIdByName,
        string? drilldownGroupName)
    {
        if (transactions.Count == 0 && budgetSeriesData == null && budgetIncomeSeriesData == null && budgetExpenseSeriesData == null) return null;

        var totals = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

        foreach (var transaction in transactions)
        {
            if (!period.IsInPeriod(DateOnly.FromDateTime(transaction.Date)))
            {
                continue;
            }
            if (!IsTransactionInMode(transaction, mode, false)) continue;

            var category = GetCategoryOrGroupLabel(transaction, isCategoryDrilldown);
            var sortAmount = GetChartSortAmount(transaction, mode);

            totals[category] = totals.TryGetValue(category, out var current)
                ? current + sortAmount
                : sortAmount;
        }

        var categoryNames = totals
            .OrderByDescending(kvp => kvp.Value)
            .Select(kvp => kvp.Key)
            .ToList();

        var amountsByCategory = new Dictionary<string, decimal[]>(StringComparer.OrdinalIgnoreCase);
        foreach (var name in categoryNames)
        {
            amountsByCategory[name] = new decimal[period.Length];
        }

        var months = period.GetPeriods().ToList();
        foreach (var t in transactions)
        {
            if (!IsTransactionInMode(t, mode, excludeNet: false)) continue;
            
            var month = months.FirstOrDefault(m => m.IsInPeriod(DateOnly.FromDateTime(t.Date)));
            var monthIndex = months.IndexOf(month!);

            var amount = GetChartStackAmount(t, mode);
            var category = GetCategoryOrGroupLabel(t, isCategoryDrilldown);

            if (amountsByCategory.TryGetValue(category, out var arr))
            {
                arr[monthIndex] += amount;
            }
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

        if (mode == TransactionsGraphMode.Net)
        {
            if (budgetIncomeSeriesData is not null && budgetIncomeSeriesData.Length == period.Length)
            {
                series.Add(CreateLineSeries(incomeBudgetName, budgetIncomeSeriesData, 7));
            }

            if (budgetExpenseSeriesData is not null && budgetExpenseSeriesData.Length == period.Length)
            {
                series.Add(CreateLineSeries(expenseBudgetName, budgetExpenseSeriesData, 7));
            }
        }
        else
        {
            if (budgetSeriesData is not null && budgetSeriesData.Length == period.Length)
            {
                series.Add(CreateLineSeries(budgetName, budgetSeriesData, 8));
            }
        }

        var titleText = (mode, isCategoryDrilldown) switch
        {
            (TransactionsGraphMode.Expense, false) => "Expenses by group",
            (TransactionsGraphMode.Income, false) => "Income by group",
            (TransactionsGraphMode.Net, false) => "Net by group",
            (_, true) => $"{drilldownGroupName ?? "Group"} by category",
            _ => "Transactions"
        };

        var xAxisLabels = period.GetPeriods().Select(m => m.StartDate.ToString("yyyy-MM")).ToArray();

        return new
        {
            title = new { text = titleText },
            tooltip = new { trigger = "axis" },
            legend = new { type = "scroll" },
            grid = new { left = "3%", right = "4%", bottom = "3%", containLabel = true },
            xAxis = new { type = "category", data = xAxisLabels },
            yAxis = new { type = "value" },
            series
        };
    }

    private static object? BuildChart2(
        List<TransactionSummary> transactions,
        List<BudgetEntry> budgetEntries,
        TransactionsGraphMode mode,
        bool isCategoryDrilldown,
        Dictionary<string, Guid> groupIdByName,
        string? drilldownGroupName)
    {
        var actualTotals = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

        foreach (var t in transactions)
        {
            if (!IsTransactionInMode(t, mode, excludeNet: true)) continue;

            var name = GetCategoryOrGroupLabel(t, isCategoryDrilldown);
            var amount = GetChartSortAmount(t, mode);

            actualTotals[name] = actualTotals.TryGetValue(name, out var current)
                ? current + amount
                : amount;
        }

        var budgetTotals = budgetEntries
            .GroupBy(e => GetCategoryOrGroupLabel(e, isCategoryDrilldown), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                g => g.Key,
                g => g.Sum(e => Math.Abs(e.Amount)),
                StringComparer.OrdinalIgnoreCase);

        var rows = actualTotals.Keys
            .Union(budgetTotals.Keys, StringComparer.OrdinalIgnoreCase)
            .Select(name => new
            {
                Name = name,
                Actual = actualTotals.GetValueOrDefault(name),
                Budget = budgetTotals.GetValueOrDefault(name)
            })
            .Where(x => x.Actual != 0m || x.Budget != 0m)
            .OrderByDescending(x => x.Actual)
            .ThenByDescending(x => x.Budget)
            .ToList();

        if (rows.Count == 0) return null;

        var labels = rows.Select(r => r.Name).ToArray();

        var drilldownKeys = rows
            .Select(r =>
            {
                if (isCategoryDrilldown) return null;
                return groupIdByName.TryGetValue(r.Name, out var id) ? id.ToString() : null;
            })
            .ToArray();

        var spentValues = rows.Select(r => (double)Math.Min(r.Actual, r.Budget)).ToArray();
        var remainingValues = rows.Select(r => (double)Math.Max(r.Budget - r.Actual, 0m)).ToArray();
        var overspendValues = rows.Select(r => (double)Math.Max(r.Actual - r.Budget, 0m)).ToArray();

        object[] BuildSeriesData(double[] values) => [.. values.Select((v, i) => new { value = v, key = drilldownKeys[i] })];

        var modeText = mode == TransactionsGraphMode.Income ? "incomes" : "expenses";
        var levelText = drilldownGroupName is null ? "All" : drilldownGroupName;

        return new
        {
            title = new { text = $"{levelText} {modeText} vs budget" },
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
                    name = "Spend",
                    type = "bar",
                    stack = "total",
                    emphasis = new { focus = "series" },
                    itemStyle = new { color = "#009de0" },
                    data = BuildSeriesData(spentValues)
                },
                new
                {
                    name = "Remaining budget",
                    type = "bar",
                    stack = "total",
                    emphasis = new { focus = "series" },
                    itemStyle = new { color = "#198754" },
                    data = BuildSeriesData(remainingValues)
                },
                new
                {
                    name = "Overspend",
                    type = "bar",
                    stack = "total",
                    emphasis = new { focus = "series" },
                    itemStyle = new { color = "#dc3545" },
                    data = BuildSeriesData(overspendValues)
                }
            }
        };
    }

    private async Task<double[]?> BuildChart1BudgetSeriesAsync(ScopedPeriod period, List<Category> categories, bool isExpense)
    {
        if (categories.Count == 0) return null;

        var data = new double[period.Length];
        var months = period.GetPeriods().ToList();

        for (var i = 0; i < period.Length; i++)
        {
            var month = months[i];

            var entries = await BudgetService.GetBudget(month, categories);
            if (!entries.Any()) continue;

            var monthBudget = entries.Sum(e => !isExpense ? e.Amount : -e.Amount);
            data[i] = (double)monthBudget;
        }

        return data.All(d => d == 0) ? null : data;
    }

    private static List<T> ApplyGroupFilter<T>(IEnumerable<T> items, Guid? groupId, Func<T, Guid?> groupIdSelector)
    {
        return groupId is Guid id ? [.. items.Where(x => groupIdSelector(x) == id)] : [.. items];
    }

    private (Dictionary<string, Guid> GroupIdByName, string? DrilldownGroupName, CategoryGroup? DrilldownGroup) BuildGroupDrilldownMeta(Guid? drilldownGroupId)
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

        var groupName = t.Category?.Group?.Name;
        return string.IsNullOrWhiteSpace(groupName) ? UncategorisedLabel : groupName;
    }

    private static string GetCategoryOrGroupLabel(BudgetEntry e, bool isCategoryDrilldown)
    {
        var name = isCategoryDrilldown ? e.Category.Name : e.Category.Group.Name;
        return string.IsNullOrWhiteSpace(name) ? UncategorisedLabel : name;
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

    private static object CreateLineSeries(string name, double[] data, int symbolSize) => new
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