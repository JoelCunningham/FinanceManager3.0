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

    public FilterQuery Query { get; set; } = new()
    {
        FilterDateFrom = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-11),
        FilterDateTo = DateTime.Today,
        FilterStatus = ReviewStatus.Reviewed
    };

    public DateTime? DateFrom
    {
        get => Query.FilterDateFrom;
        set => Query.FilterDateFrom = value;
    }

    public DateTime? DateTo
    {
        get => Query.FilterDateTo;
        set => Query.FilterDateTo = value;
    }

    public TransactionsGraphMode Mode { get; set; } = TransactionsGraphMode.Expense;

    public bool ReviewedOnly
    {
        get => Query.FilterStatus == ReviewStatus.Reviewed;
        set => Query.FilterStatus = value ? ReviewStatus.Reviewed : ReviewStatus.All;
    }

    public int TopCategories { get; set; } = 10;

    public bool IsLoading { get; set; }
    public string? LoadError { get; set; }
    public object? ChartOption { get; set; }

    public IReadOnlyList<TransactionSummary> TransactionSummaries { get; set; } = [];
    public IReadOnlyList<Category> Categories { get; set; } = [];

    protected override async Task OnInitializedAsync()
    {
        Categories = [.. (await CategoryService.GetCategories())];
        await ReloadAsync();
    }

    public async Task ReloadAsync()
    {
        IsLoading = true;
        LoadError = null;
        ChartOption = null;

        try
        {
            if (DateFrom is null || DateTo is null)
            {
                return;
            }

            TransactionSummaries = [.. (await TransactionService.GetAllAsync<TransactionSummary>(Query))];

            var months = GetMonths(DateFrom.Value, DateTo.Value);

            double[]? budgetSeriesData = null;
            double[]? budgetIncomeSeriesData = null;
            double[]? budgetExpenseSeriesData = null;

            if (Mode == TransactionsGraphMode.Net)
            {
                var incomeCategories = Categories.Where(c => c.Group.IsIncome).ToList();
                var expenseCategories = Categories.Where(c => !c.Group.IsIncome).ToList();

                budgetIncomeSeriesData = await BuildMonthlyBudgetSeriesAsync(months, incomeCategories, entry => entry.Amount);
                budgetExpenseSeriesData = await BuildMonthlyBudgetSeriesAsync(months, expenseCategories, entry => -entry.Amount);
            }
            else
            {
                var relevantCategories = Mode == TransactionsGraphMode.Income
                    ? Categories.Where(c => c.Group.IsIncome).ToList()
                    : Categories.Where(c => !c.Group.IsIncome).ToList();

                budgetSeriesData = await BuildMonthlyBudgetSeriesAsync(months, relevantCategories, entry => TransformBudgetAmount(entry, Mode));
            }

            ChartOption = BuildMonthlyStackedCategoryChart(
                TransactionSummaries,
                months,
                DateTo.Value,
                Mode,
                TopCategories,
                budgetSeriesData,
                budgetIncomeSeriesData,
                budgetExpenseSeriesData);
        }
        catch (Exception ex)
        {
            LoadError = ex.Message;
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    private static List<DateTime> GetMonths(DateTime dateFrom, DateTime dateTo)
    {
        var start = new DateTime(dateFrom.Year, dateFrom.Month, 1);
        var end = new DateTime(dateTo.Year, dateTo.Month, 1);

        var months = new List<DateTime>();
        for (var d = start; d <= end; d = d.AddMonths(1))
        {
            months.Add(d);
        }

        return months;
    }

    private async Task<double[]?> BuildMonthlyBudgetSeriesAsync(List<DateTime> months, List<Category> categories, Func<BudgetEntry, decimal> amountSelector)
    {
        if (categories.Count == 0 || months.Count == 0)
        {
            return null;
        }

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

    private static decimal TransformBudgetAmount(BudgetEntry entry, TransactionsGraphMode mode)
    {
        var isIncome = entry.Category.Group.IsIncome;

        return mode switch
        {
            TransactionsGraphMode.Expense => isIncome ? 0m : entry.Amount,
            TransactionsGraphMode.Income => isIncome ? entry.Amount : 0m,
            TransactionsGraphMode.Net => isIncome ? entry.Amount : -entry.Amount,
            _ => entry.Amount
        };
    }

    private static object? BuildMonthlyStackedCategoryChart(
        IReadOnlyList<TransactionSummary> transactions,
        List<DateTime> months,
        DateTime dateTo,
        TransactionsGraphMode mode,
        int topCategories,
        double[]? budgetSeriesData,
        double[]? budgetIncomeSeriesData,
        double[]? budgetExpenseSeriesData)
    {
        if (transactions.Count == 0 || months.Count == 0)
        {
            return null;
        }

        var start = months[0];

        static string MonthLabel(DateTime d) => d.ToString("yyyy-MM");

        static decimal TransformAmount(decimal amount, TransactionsGraphMode graphMode) => graphMode switch
        {
            TransactionsGraphMode.Expense => amount < 0 ? Math.Abs(amount) : 0m,
            TransactionsGraphMode.Income => amount > 0 ? amount : 0m,
            TransactionsGraphMode.Net => amount,
            _ => amount
        };

        var points = transactions
            .Where(t => t.Date >= start && t.Date <= dateTo)
            .Select(t => new
            {
                Month = new DateTime(t.Date.Year, t.Date.Month, 1),
                Category = t.Category?.Name ?? "Uncategorised",
                Value = TransformAmount(t.Amount, mode)
            })
            .Where(p => p.Value != 0m)
            .ToList();

        if (points.Count == 0)
        {
            return null;
        }

        var totalsByCategory = points
            .GroupBy(p => p.Category)
            .Select(g => new { Category = g.Key, Total = g.Sum(x => Math.Abs(x.Value)) })
            .OrderByDescending(x => x.Total)
            .ToList();

        topCategories = Math.Clamp(topCategories, 1, 30);

        var selectedCategories = totalsByCategory
            .Take(topCategories)
            .Select(x => x.Category)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var categories = totalsByCategory
            .Where(x => selectedCategories.Contains(x.Category))
            .Select(x => x.Category)
            .ToList();

        var hasOther = totalsByCategory.Count > categories.Count;

        var monthIndex = months
            .Select((m, i) => (m, i))
            .ToDictionary(x => x.m, x => x.i);

        var valuesByCategory = new Dictionary<string, decimal[]>(StringComparer.OrdinalIgnoreCase);
        foreach (var c in categories)
        {
            valuesByCategory[c] = new decimal[months.Count];
        }

        decimal[]? otherValues = hasOther ? new decimal[months.Count] : null;

        foreach (var p in points)
        {
            if (!monthIndex.TryGetValue(p.Month, out var i))
            {
                continue;
            }

            if (valuesByCategory.TryGetValue(p.Category, out var arr))
            {
                arr[i] += p.Value;
            }
            else
            {
                otherValues?[i] += p.Value;
            }
        }

        var xAxisLabels = months.Select(MonthLabel).ToArray();
        var series = new List<object>();

        foreach (var c in categories)
        {
            series.Add(new
            {
                name = c,
                type = "bar",
                stack = "total",
                emphasis = new { focus = "series" },
                data = valuesByCategory[c].Select(v => (double)v).ToArray()
            });
        }

        if (otherValues is not null)
        {
            series.Add(new
            {
                name = "Other",
                type = "bar",
                stack = "total",
                emphasis = new { focus = "series" },
                data = otherValues.Select(v => (double)v).ToArray()
            });
        }

        if (mode == TransactionsGraphMode.Net)
        {
            if (budgetIncomeSeriesData is not null && budgetIncomeSeriesData.Length == months.Count)
            {
                series.Add(new
                {
                    name = "Budget (Income)",
                    type = "line",
                    smooth = true,
                    symbol = "circle",
                    symbolSize = 7,
                    z = 20,
                    lineStyle = new { width = 3 },
                    data = budgetIncomeSeriesData
                });
            }

            if (budgetExpenseSeriesData is not null && budgetExpenseSeriesData.Length == months.Count)
            {
                series.Add(new
                {
                    name = "Budget (Expense)",
                    type = "line",
                    smooth = true,
                    symbol = "circle",
                    symbolSize = 7,
                    z = 20,
                    lineStyle = new { width = 3, type = "dashed" },
                    data = budgetExpenseSeriesData
                });
            }
        }
        else
        {
            if (budgetSeriesData is not null && budgetSeriesData.Length == months.Count)
            {
                series.Add(new
                {
                    name = "Budget",
                    type = "line",
                    smooth = true,
                    symbol = "circle",
                    symbolSize = 8,
                    z = 10,
                    lineStyle = new { width = 3 },
                    data = budgetSeriesData
                });
            }
        }

        var titleText = mode switch
        {
            TransactionsGraphMode.Expense => "Expenses by category",
            TransactionsGraphMode.Income => "Income by category",
            TransactionsGraphMode.Net => "Net by category",
            _ => "Transactions by category"
        };

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
}