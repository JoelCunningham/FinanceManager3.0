namespace FinanceManager.Application.UseCases.Categories;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Enums;
using FinanceManager.Application.UseCases;
using FinanceManager.Application.Utilities;

public sealed record GetCategoryChartResult(object[] Serieses, string[] Labels, string Title) : UseCaseResult;

public sealed class GetCategoryChart(ChartHelper chartHelper)
{
    public async Task<GetCategoryChartResult> ExecuteAsync(IEnumerable<CategorySummary> categories, IEnumerable<ScopedPeriod> periods, ChartMode mode, CategoryGroupDetails group)
    {
        var serieses = new List<object>();
        var transactionSerieses = await chartHelper.GetTransactionsPerCategoryAndPeriod(categories, periods);
        var budgetSerieses = await chartHelper.GetBudgetsPerCategoryAndPeriod(categories, periods);

        foreach (var transactionSeries in transactionSerieses)
        {
            var category = categories.FirstOrDefault(c => c.Id == transactionSeries.Key);
            if (category is null) continue;

            budgetSerieses.TryGetValue(transactionSeries.Key, out var budgetSeries);
            if (budgetSeries is null) continue;

            if (mode != ChartMode.Budget && mode != ChartMode.Variance)
            {
                var series = ChartSeries.LineSeries(category.Name, category.Colour, transactionSeries.Value.Select(b => group.IsIncome ? b : -b));
                serieses.AddRange(series.ToEChartsSeries());
            }
            if (mode != ChartMode.Actual && mode != ChartMode.Variance)
            {
                var series = ChartSeries.LineSeries(category.Name, category.Colour, budgetSeries, true, "Budget");
                serieses.AddRange(series.ToEChartsSeries());
            }
            if (mode == ChartMode.Variance)
            {
                var series = ChartSeries.LineSeries(category.Name, category.Colour, transactionSeries.Value.Zip(budgetSeries, (t, b) => group.IsIncome ? t - b : b - t));
                serieses.AddRange(series.ToEChartsSeries());
            }
        }

        var labels = periods.Select(p => p.StartDate.ToString("yyyy-MM"));
        var title = $"{group.Name} categories actual vs budget for {periods.First().StartDate:MMM yyyy} - {periods.Last().EndDate:MMM yyyy}";

        return new GetCategoryChartResult([.. serieses], [.. labels], title);
    }
}
