namespace FinanceManager.Application.UseCases.Categories;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Enums;
using FinanceManager.Application.UseCases;
using FinanceManager.Application.Utilities;

public sealed record GetCategoryChartResult(ChartOptions Options) : UseCaseResult;

public sealed class GetCategoryChart(ChartHelper chartHelper)
{
    public async Task<GetCategoryChartResult> ExecuteAsync(IEnumerable<CategorySummary> categories, IEnumerable<ScopedPeriod> periods, ChartMode mode, CategoryGroupDetails group)
    {
        ChartOptions options = new(hideLegend: true);

        var transactionSerieses = await chartHelper.GetTransactionsPerCategoryAndPeriod(categories, periods);
        var budgetSerieses = await chartHelper.GetBudgetsPerCategoryAndPeriod(categories, periods);

        foreach (var transactionSeries in transactionSerieses)
        {
            var category = categories.FirstOrDefault(c => c.Id == transactionSeries.Key);
            if (category is null) continue;

            budgetSerieses.TryGetValue(transactionSeries.Key, out var budgetSeries);
            budgetSeries ??= [.. Enumerable.Repeat(0m, periods.Count())];

            if (mode != ChartMode.Budget && mode != ChartMode.Variance)
            {
                options.AddSeries(ChartSeries.LineSeries(category.Name, category.Colour, transactionSeries.Value.Select(b => group.IsIncome ? b : -b)));
            }
            if (mode != ChartMode.Actual && mode != ChartMode.Variance)
            {
                options.AddSeries(ChartSeries.LineSeries(category.Name, category.Colour, budgetSeries, true, "Budget"));
            }
            if (mode == ChartMode.Variance)
            {
                options.AddSeries(ChartSeries.LineSeries(category.Name, category.Colour, transactionSeries.Value.Zip(budgetSeries, (t, b) => group.IsIncome ? t - b : b - t)));
            }
        }

        options.SetLabels(periods.Select(p => p.StartDate.ToString("yyyy-MM")));

        return new GetCategoryChartResult(options);
    }
}
