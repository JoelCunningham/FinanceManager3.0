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

    public object? ChartOption { get; set; }
    public List<Category> Categories { get; set; } = [];
    public TransactionsGraphMode Mode { get; set; } = TransactionsGraphMode.Expense;
    public List<TransactionSummary> TransactionSummaries { get; set; } = [];
    public IReadOnlyList<DateTime> AvailableMonths { get; } = BuildAvailableMonths(10);

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
    }

    public async Task ReloadAsync()
    {
        ChartOption = null;

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

        if (Mode == TransactionsGraphMode.Net)
        {
            budgetIncomeSeriesData = await BuildMonthlyBudgetSeriesAsync(months, incomeCategories, static entry => entry.Amount);
            budgetExpenseSeriesData = await BuildMonthlyBudgetSeriesAsync(months, expenseCategories, static entry => -entry.Amount);
        }
        else
        {
            var relevantCategories = Mode == TransactionsGraphMode.Income ? incomeCategories : expenseCategories;
            budgetSeriesData = await BuildMonthlyBudgetSeriesAsync(months, relevantCategories, static entry => entry.Amount);
        }

        ChartOption = BuildMonthlyStackedCategoryChart(
            TransactionSummaries,
            months,
            Mode,
            budgetSeriesData,
            budgetIncomeSeriesData,
            budgetExpenseSeriesData);

        StateHasChanged();
    }

    private static List<DateTime> BuildAvailableMonths(int yearsBack)
    {
        var current = NormalizeMonth(DateTime.Today);
        var start = current.AddYears(-yearsBack);

        var months = new List<DateTime>();
        for (var d = start; d <= current; d = d.AddMonths(1))
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

    private async Task<double[]?> BuildMonthlyBudgetSeriesAsync(
        List<DateTime> months,
        List<Category> categories,
        Func<BudgetEntry, decimal> amountSelector)
    {
        if (categories.Count == 0 || months.Count == 0) return null;

        var data = new double[months.Count];

        for (var i = 0; i < months.Count; i++)
        {
            var monthStart = DateOnly.FromDateTime(months[i]);
            var entries = await BudgetService.GetBudgets(monthStart, categories, TransactionLevel.Month);

            var monthBudget = entries.Sum(amountSelector);
            data[i] = (double)monthBudget;
        }

        return data;
    }

    private static object? BuildMonthlyStackedCategoryChart(
        List<TransactionSummary> transactions,
        List<DateTime> months,
        TransactionsGraphMode mode,
        double[]? budgetSeriesData,
        double[]? budgetIncomeSeriesData,
        double[]? budgetExpenseSeriesData)
    {
        if (transactions.Count == 0 || months.Count == 0) return null;

        var totals = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
        var monthIndex = months.Select((m, i) => (m, i)).ToDictionary(x => x.m, x => x.i);

        foreach (var t in transactions)
        {
            var month = new DateTime(t.Date.Year, t.Date.Month, 1);
            if (!monthIndex.ContainsKey(month)) continue;

            if (mode == TransactionsGraphMode.Income && t.Amount <= 0m) continue;
            if (mode == TransactionsGraphMode.Expense && t.Amount >= 0m) continue;

            var category = t.Category?.Name ?? "Uncategorised";
            var amount = mode == TransactionsGraphMode.Expense ? Math.Abs(t.Amount) : t.Amount;

            totals[category] = totals.TryGetValue(category, out var current)
                ? current + Math.Abs(amount)
                : Math.Abs(amount);
        }

        if (totals.Count == 0) return null;

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
            var category = t.Category?.Name ?? "Uncategorised";

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