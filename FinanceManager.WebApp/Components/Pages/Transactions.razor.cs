namespace FinanceManager.WebApp.Components.Pages;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Services;
using FinanceManager.Domain.Entities;
using FinanceManager.Domain.Enums;
using FinanceManager.WebApp.Enums;
using Microsoft.AspNetCore.Components;

public partial class Transactions : ComponentBase
{
    [Inject] public TransactionService TransactionService { get; set; } = default!;
    [Inject] public CategoryService CategoryService { get; set; } = default!;
    [Inject] public BudgetService BudgetService { get; set; } = default!;

    public List<Category> Categories { get; set; } = [];
    private IReadOnlyList<CategoryGroup> CategoryGroups => [.. Categories.Select(c => c.Group).DistinctBy(g => g.Id).OrderBy(g => g.Name)];

    public List<TransactionSummary> TransactionSummaries { get; set; } = [];
    public IReadOnlyList<DateTime> AvailableMonths { get; } = BuildAvailableMonths(10);

    public object? Chart1Options { get; set; }
    public TransactionsGraphMode Chart1Mode { get; set; } = TransactionsGraphMode.Expense;
    public Guid? Chart1DrilldownGroupId { get; set; }

    public object? Chart2Options { get; set; }
    public TransactionsGraphRange Chart2Range { get; set; } = TransactionsGraphRange.Month;
    public DateTime Chart2Period { get; set; } = DateTime.Today;
    public TransactionsGraphMode Chart2Mode { get; set; } = TransactionsGraphMode.Expense;
    public Guid? Chart2DrilldownGroupId { get; set; }

    public FilterQuery Query { get; set; } = new()
    {
        FilterDateFrom = CurrentMonth.AddMonths(-11),
        FilterDateTo = DateTime.Today,
        FilterStatus = ReviewStatus.Reviewed
    };

    public DateTime MonthFrom
    {
        get => _monthFrom;
        set
        {
            _monthFrom = NormalizeMonth(value);
            if (_monthTo < _monthFrom) _monthTo = _monthFrom;
        }
    }

    public DateTime MonthTo
    {
        get => _monthTo;
        set
        {
            _monthTo = NormalizeMonth(value);
            if (_monthTo < _monthFrom) _monthFrom = _monthTo;
        }
    }

    private DateTime _monthFrom = CurrentMonth.AddMonths(-11);
    private DateTime _monthTo = CurrentMonth;

    private static readonly DateTime CurrentMonth = NormalizeMonth(DateTime.Today);
    private static DateTime EndOfMonth(DateTime monthStart) => NormalizeMonth(monthStart).AddMonths(1).AddTicks(-1);
    private static DateTime NormalizeMonth(DateTime value) => new(value.Year, value.Month, 1);

    protected override async Task OnInitializedAsync()
    {
        Categories = [.. (await CategoryService.GetCategories())];

        await ReloadAsync();
        await ReloadChart2Async();
    }

    public async Task ReloadAsync()
    {
        Chart1Options = null;

        var months = GetMonths(MonthFrom, MonthTo);
        var monthToEnd = EndOfMonth(MonthTo);

        Query.FilterDateFrom = MonthFrom;
        Query.FilterDateTo = monthToEnd > DateTime.Today ? DateTime.Today : monthToEnd;

        TransactionSummaries = [.. await TransactionService.GetAllAsync<TransactionSummary>(Query)];

        var drilldownGroupId = Chart1DrilldownGroupId;

        var chartTransactions = TransactionSummaries;
        if (drilldownGroupId is Guid groupId)
        {
            chartTransactions = [.. chartTransactions.Where(t => t.Category?.GroupId == groupId)];
        }

        var incomeCategories = Categories.Where(c => c.Group.IsIncome).ToList();
        var expenseCategories = Categories.Where(c => !c.Group.IsIncome).ToList();

        // If we're drilled into a group, only budget that group's categories.
        if (drilldownGroupId is Guid drilledId)
        {
            incomeCategories = [.. incomeCategories.Where(c => c.GroupId == drilledId)];
            expenseCategories = [.. expenseCategories.Where(c => c.GroupId == drilledId)];
        }

        var groupIdByName = CategoryGroups.ToDictionary(g => g.Name, g => g.Id, StringComparer.OrdinalIgnoreCase);

        var drilldownGroupName = drilldownGroupId is Guid drilledGroupId
            ? CategoryGroups.FirstOrDefault(g => g.Id == drilledGroupId)?.Name
            : null;

        double[]? budgetSeriesData = null;
        double[]? budgetIncomeSeriesData = null;
        double[]? budgetExpenseSeriesData = null;

        if (Chart1Mode == TransactionsGraphMode.Net)
        {
            if (drilldownGroupId is null)
            {
                budgetIncomeSeriesData = await BuildMonthlyBudgetSeriesAsync(months, incomeCategories, false);
                budgetExpenseSeriesData = await BuildMonthlyBudgetSeriesAsync(months, expenseCategories, true);
            }
            else
            {
                // In drilldown, only show the relevant side (income OR expense) if possible.
                var drilledGroup = drilldownGroupId is Guid id
                    ? CategoryGroups.FirstOrDefault(g => g.Id == id)
                    : null;

                if (drilledGroup?.IsIncome == true)
                {
                    budgetIncomeSeriesData = await BuildMonthlyBudgetSeriesAsync(months, incomeCategories, false);
                }
                else if (drilledGroup?.IsIncome == false)
                {
                    budgetExpenseSeriesData = await BuildMonthlyBudgetSeriesAsync(months, expenseCategories, true);
                }
                else
                {
                    budgetIncomeSeriesData = await BuildMonthlyBudgetSeriesAsync(months, incomeCategories, false);
                    budgetExpenseSeriesData = await BuildMonthlyBudgetSeriesAsync(months, expenseCategories, true);
                }
            }
        }
        else
        {
            var relevantCategories = Chart1Mode == TransactionsGraphMode.Income ? incomeCategories : expenseCategories;
            budgetSeriesData = await BuildMonthlyBudgetSeriesAsync(months, relevantCategories, false);
        }

        Chart1Options = BuildMonthlyStackedCategoryChart(
            chartTransactions,
            months,
            Chart1Mode,
            budgetSeriesData,
            budgetIncomeSeriesData,
            budgetExpenseSeriesData,
            isCategoryDrilldown: drilldownGroupId is not null,
            groupIdByName: groupIdByName,
            drilldownGroupName: drilldownGroupName);

        StateHasChanged();
    }

    private async Task OnChart1ItemClickedAsync(string? key)
    {
        // If already drilled down, first click takes you back to groups (same pattern as Chart2).
        if (Chart1DrilldownGroupId is not null)
        {
            await ClearChart1DrilldownAsync();
        }

        if (!Guid.TryParse(key, out var groupId)) return;
        Chart1DrilldownGroupId = groupId;

        await ReloadAsync();
    }

    private async Task ClearChart1DrilldownAsync()
    {
        Chart1DrilldownGroupId = null;
        await ReloadAsync();
    }

    public async Task ReloadChart2Async()
    {
        Chart2Options = null;

        var (rangeStart, rangeEnd) = GetChart2DateRange(Chart2Range, Chart2Period);
        var todayEnd = DateTime.Today.AddDays(1).AddTicks(-1);
        if (rangeEnd > todayEnd) rangeEnd = todayEnd;

        var drilldownGroupId = Chart2DrilldownGroupId;

        var relevantCategories = Chart2Mode == TransactionsGraphMode.Income
            ? Categories.Where(c => c.Group.IsIncome).ToList()
            : Categories.Where(c => !c.Group.IsIncome).ToList();

        if (drilldownGroupId is Guid groupId)
        {
            relevantCategories = [.. relevantCategories.Where(c => c.GroupId == groupId)];
        }

        var query = new FilterQuery
        {
            FilterDateFrom = rangeStart,
            FilterDateTo = rangeEnd,
            FilterStatus = ReviewStatus.Reviewed
        };

        var transactions = (await TransactionService.GetAllAsync<TransactionSummary>(query)).ToList();

        if (drilldownGroupId is not null)
        {
            transactions = [.. transactions.Where(t => t.Category?.GroupId == drilldownGroupId)];
        }

        var budgetEntries = (await BudgetService.GetBudget(
                DateOnly.FromDateTime(rangeStart),
                DateOnly.FromDateTime(rangeEnd),
                relevantCategories))
            .ToList();

        var groupIdByName = CategoryGroups.ToDictionary(g => g.Name, g => g.Id, StringComparer.OrdinalIgnoreCase);

        var drilldownGroupName = drilldownGroupId is Guid drilledId
            ? CategoryGroups.FirstOrDefault(g => g.Id == drilledId)?.Name
            : null;

        Chart2Options = BuildActualVsBudgetStackedChart(
            transactions,
            budgetEntries,
            Chart2Mode,
            isCategoryDrilldown: drilldownGroupId is not null,
            groupIdByName: groupIdByName,
            drilldownGroupName: drilldownGroupName);

        StateHasChanged();
    }

    private async Task OnChart2ItemClickedAsync(string? key)
    {
        if (Chart2DrilldownGroupId is not null)
        {
            await ClearChart2DrilldownAsync();
        }

        if (!Guid.TryParse(key, out var groupId)) return;
        Chart2DrilldownGroupId = groupId;

        await ReloadChart2Async();
    }

    private async Task ClearChart2DrilldownAsync()
    {
        Chart2DrilldownGroupId = null;
        await ReloadChart2Async();
    }

    private static (DateTime Start, DateTime End) GetChart2DateRange(TransactionsGraphRange range, DateTime anchor)
    {
        var date = anchor.Date;

        return range switch
        {
            TransactionsGraphRange.Week => (StartOfWeek(date), StartOfWeek(date).AddDays(7).AddTicks(-1)),
            TransactionsGraphRange.Fortnight => (StartOfWeek(date), StartOfWeek(date).AddDays(14).AddTicks(-1)),
            TransactionsGraphRange.Month => (new DateTime(date.Year, date.Month, 1), new DateTime(date.Year, date.Month, 1).AddMonths(1).AddTicks(-1)),
            _ => (date, date.AddDays(1).AddTicks(-1))
        };
    }

    private static DateTime StartOfWeek(DateTime date)
    {
        var diff = (7 + (int)date.DayOfWeek - (int)DayOfWeek.Monday) % 7;
        return date.AddDays(-diff);
    }

    private static object? BuildActualVsBudgetStackedChart(
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
            if (mode == TransactionsGraphMode.Income && t.Amount <= 0m) continue;
            if (mode == TransactionsGraphMode.Expense && t.Amount >= 0m) continue;
            if (mode == TransactionsGraphMode.Net) continue;

            var name = isCategoryDrilldown
                ? (t.Category?.Name ?? "Uncategorised")
                : (t.Category?.Group.Name ?? "Uncategorised");

            var amount = mode == TransactionsGraphMode.Expense ? Math.Abs(t.Amount) : t.Amount;

            actualTotals[name] = actualTotals.TryGetValue(name, out var current)
                ? current + Math.Abs(amount)
                : Math.Abs(amount);
        }

        var budgetTotals = budgetEntries
            .GroupBy(e => isCategoryDrilldown ? e.Category.Name : e.Category.Group.Name, StringComparer.OrdinalIgnoreCase)
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

    private static List<DateTime> BuildAvailableMonths(int yearsBack)
    {
        var current = NormalizeMonth(DateTime.Today);
        var start = current.AddYears(-yearsBack);
        var end = current.AddYears(1);

        var months = new List<DateTime>();
        for (var d = start; d <= end; d = d.AddMonths(1))
        {
            months.Add(d);
        }

        return months;
    }

    private static List<DateTime> GetMonths(DateTime dateFrom, DateTime dateTo)
    {
        var start = NormalizeMonth(dateFrom);
        var end = NormalizeMonth(dateTo);

        var months = new List<DateTime>();
        for (var d = start; d <= end; d = d.AddMonths(1))
        {
            months.Add(d);
        }

        return months;
    }

    private async Task<double[]?> BuildMonthlyBudgetSeriesAsync(List<DateTime> months, List<Category> categories, bool isExpense)
    {
        if (categories.Count == 0 || months.Count == 0) return null;

        var data = new double[months.Count];

        for (var i = 0; i < months.Count; i++)
        {
            var monthStart = DateOnly.FromDateTime(months[i]);
            var entries = await BudgetService.GetBudgets(monthStart, categories, TransactionLevel.Month);

            if (entries is null || !entries.Any()) continue;

            var monthBudget = entries.Sum(e => !isExpense ? e.Amount : -e.Amount);
            data[i] = (double)monthBudget;
        }

        return data.All(d => d == 0) ? null : data;
    }

    private static object? BuildMonthlyStackedCategoryChart(
        List<TransactionSummary> transactions,
        List<DateTime> months,
        TransactionsGraphMode mode,
        double[]? budgetSeriesData,
        double[]? budgetIncomeSeriesData,
        double[]? budgetExpenseSeriesData,
        bool isCategoryDrilldown,
        Dictionary<string, Guid> groupIdByName,
        string? drilldownGroupName)
    {
        if (months.Count == 0) return null;
        if (transactions.Count == 0 && budgetSeriesData == null && budgetIncomeSeriesData == null && budgetExpenseSeriesData == null) return null;

        var totals = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
        var monthIndex = months.Select((m, i) => (m, i)).ToDictionary(x => x.m, x => x.i);

        foreach (var t in transactions)
        {
            var month = new DateTime(t.Date.Year, t.Date.Month, 1);
            if (!monthIndex.ContainsKey(month)) continue;

            if (mode == TransactionsGraphMode.Income && t.Amount <= 0m) continue;
            if (mode == TransactionsGraphMode.Expense && t.Amount >= 0m) continue;

            var category = GetChart1Label(t, isCategoryDrilldown);
            var amount = mode == TransactionsGraphMode.Expense ? Math.Abs(t.Amount) : t.Amount;

            totals[category] = totals.TryGetValue(category, out var current)
                ? current + Math.Abs(amount)
                : Math.Abs(amount);
        }

        var categoryNames = totals
            .OrderByDescending(kvp => kvp.Value)
            .Select(kvp => kvp.Key)
            .ToList();

        var amountsByCategory = new Dictionary<string, decimal[]>(StringComparer.OrdinalIgnoreCase);
        foreach (var name in categoryNames)
        {
            amountsByCategory[name] = new decimal[months.Count];
        }

        foreach (var t in transactions)
        {
            if (mode == TransactionsGraphMode.Income && t.Amount <= 0m) continue;
            if (mode == TransactionsGraphMode.Expense && t.Amount >= 0m) continue;

            var month = new DateTime(t.Date.Year, t.Date.Month, 1);
            if (!monthIndex.TryGetValue(month, out var i)) continue;

            var amount = mode == TransactionsGraphMode.Expense ? Math.Abs(t.Amount) : t.Amount;
            var category = GetChart1Label(t, isCategoryDrilldown);

            if (amountsByCategory.TryGetValue(category, out var arr))
            {
                arr[i] += amount;
            }
        }

        var series = new List<object>();

        foreach (var name in categoryNames)
        {
            // Only group-level series are clickable (they carry a key).
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
            if (budgetIncomeSeriesData is not null && budgetIncomeSeriesData.Length == months.Count)
            {
                series.Add(CreateLineSeries(incomeBudgetName, budgetIncomeSeriesData, 7));
            }

            if (budgetExpenseSeriesData is not null && budgetExpenseSeriesData.Length == months.Count)
            {
                series.Add(CreateLineSeries(expenseBudgetName, budgetExpenseSeriesData, 7));
            }
        }
        else
        {
            if (budgetSeriesData is not null && budgetSeriesData.Length == months.Count)
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

        var xAxisLabels = months.Select(m => m.ToString("yyyy-MM")).ToArray();

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

    private static string GetChart1Label(TransactionSummary t, bool isCategoryDrilldown)
    {
        if (isCategoryDrilldown)
        {
            var categoryName = t.Category?.Name;
            return string.IsNullOrWhiteSpace(categoryName) ? "Uncategorised" : categoryName;
        }

        var groupName = t.Category?.Group?.Name;
        return string.IsNullOrWhiteSpace(groupName) ? "Uncategorised" : groupName;
    }

    static object CreateBarSeries(string name, decimal[] values, string? key) => new
    {
        name,
        type = "bar",
        stack = "total",
        emphasis = new { focus = "series" },
        data = key is null
            ? values.Select(v => (object)(double)v).ToArray()
            : values.Select(v => (object)new { value = (double)v, key }).ToArray()
    };

    static object CreateLineSeries(string name, double[] data, int symbolSize) => new
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