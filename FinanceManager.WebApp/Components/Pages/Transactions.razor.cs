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

        var incomeCategories = Categories.Where(c => c.Group.IsIncome).ToList();
        var expenseCategories = Categories.Where(c => !c.Group.IsIncome).ToList();

        double[]? budgetSeriesData = null;
        double[]? budgetIncomeSeriesData = null;
        double[]? budgetExpenseSeriesData = null;

        if (Chart1Mode == TransactionsGraphMode.Net)
        {
            budgetIncomeSeriesData = await BuildMonthlyBudgetSeriesAsync(months, incomeCategories, false);
            budgetExpenseSeriesData = await BuildMonthlyBudgetSeriesAsync(months, expenseCategories, true);
        }
        else
        {
            var relevantCategories = Chart1Mode == TransactionsGraphMode.Income ? incomeCategories : expenseCategories;
            budgetSeriesData = await BuildMonthlyBudgetSeriesAsync(months, relevantCategories, false);
        }

        Chart1Options = BuildMonthlyStackedCategoryChart(
            TransactionSummaries,
            months,
            Chart1Mode,
            budgetSeriesData,
            budgetIncomeSeriesData,
            budgetExpenseSeriesData);

        StateHasChanged();
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
        double[]? budgetExpenseSeriesData)
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

            var category = GetChart1CategoryLabel(t);
            var amount = mode == TransactionsGraphMode.Expense ? Math.Abs(t.Amount) : t.Amount;

            totals[category] = totals.TryGetValue(category, out var current)
                ? current + Math.Abs(amount)
                : Math.Abs(amount);
        }

        var categories = totals
            .OrderByDescending(kvp => kvp.Value)
            .Select(kvp => kvp.Key)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var categoryNames = totals
            .Where(kvp => categories.Contains(kvp.Key))
            .OrderByDescending(kvp => kvp.Value)
            .Select(kvp => kvp.Key)
            .ToList();

        var amountsByCategory = new Dictionary<string, decimal[]>(StringComparer.OrdinalIgnoreCase);
        foreach (var name in categoryNames)
        {
            amountsByCategory[name] = new decimal[months.Count];
        }

        var otherValues = totals.Count > categoryNames.Count ? new decimal[months.Count] : null;

        foreach (var t in transactions)
        {
            if (mode == TransactionsGraphMode.Income && t.Amount <= 0m) continue;
            if (mode == TransactionsGraphMode.Expense && t.Amount >= 0m) continue;

            var month = new DateTime(t.Date.Year, t.Date.Month, 1);
            if (!monthIndex.TryGetValue(month, out var i)) continue;

            var amount = mode == TransactionsGraphMode.Expense ? Math.Abs(t.Amount) : t.Amount;
            var category = GetChart1CategoryLabel(t);

            if (amountsByCategory.TryGetValue(category, out var arr))
            {
                arr[i] += amount;
            }
            else
            {
                otherValues?[i] += amount;
            }
        }

        var series = new List<object>();

        foreach (var name in categoryNames)
        {
            series.Add(CreateBarSeries(name, amountsByCategory[name]));
        }

        if (otherValues is not null)
        {
            series.Add(CreateBarSeries("Other", otherValues));
        }

        if (mode == TransactionsGraphMode.Net)
        {
            if (budgetIncomeSeriesData is not null && budgetIncomeSeriesData.Length == months.Count)
            {
                series.Add(CreateLineSeries("Budget (Income)", budgetIncomeSeriesData, 7));
            }

            if (budgetExpenseSeriesData is not null && budgetExpenseSeriesData.Length == months.Count)
            {
                series.Add(CreateLineSeries("Budget (Expense)", budgetExpenseSeriesData, 7));
            }
        }
        else
        {
            if (budgetSeriesData is not null && budgetSeriesData.Length == months.Count)
            {
                series.Add(CreateLineSeries("Budget", budgetSeriesData, 8));
            }
        }

        var titleText = mode switch
        {
            TransactionsGraphMode.Expense => "Expenses by category",
            TransactionsGraphMode.Income => "Income by category",
            TransactionsGraphMode.Net => "Net by category",
            _ => "Transactions by category"
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

    private static string GetChart1CategoryLabel(TransactionSummary t)
    {
        var categoryName = t.Category?.Name;
        if (string.IsNullOrWhiteSpace(categoryName)) return "Uncategorised";

        var groupName = t.Category?.Group?.Name;
        if (string.IsNullOrWhiteSpace(groupName)) return categoryName;

        return $"{groupName} - {categoryName}";
    }

    static object CreateBarSeries(string name, decimal[] values) => new
    {
        name,
        type = "bar",
        stack = "total",
        emphasis = new { focus = "series" },
        data = values.Select(v => (double)v).ToArray()
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