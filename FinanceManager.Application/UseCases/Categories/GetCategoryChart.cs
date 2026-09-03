namespace FinanceManager.Application.UseCases.Categories;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Enums;
using FinanceManager.Application.UseCases;
using FinanceManager.Application.Utilities;

public sealed record GetCategoryChartResult(object[] Series) : UseCaseResult;

public sealed class GetCategoryChart(ChartHelper chartHelper)
{
    public async Task<GetCategoryChartResult> ExecuteAsync(IEnumerable<CategorySummary> categories, IEnumerable<ScopedPeriod> periods, CategoryChartMode mode, bool isIncome)
    {
        var serieses = new List<object>();

        foreach (var category in categories)
        {
            var budgetSeries = await chartHelper.GetBudgetsPerPeriod(category, periods);
            var transactionSeries = await chartHelper.GetTransactionsPerPeriod(category, periods);

            if (mode != CategoryChartMode.Budget && mode != CategoryChartMode.Variance)
            {
                var series = ChartSeries.LineSeries(category.Name, category.Colour, transactionSeries.Select(b => isIncome ? b : -b));
                serieses.AddRange(series.ToEChartsSeries());
            }
            if (mode != CategoryChartMode.Actual && mode != CategoryChartMode.Variance)
            {
                var series = ChartSeries.LineDashedSeries(category.Name, category.Colour, budgetSeries);
                serieses.AddRange(series.ToEChartsSeries());
            }
            if (mode == CategoryChartMode.Variance)
            {
                var series = ChartSeries.LineSeries(category.Name, category.Colour, transactionSeries.Zip(budgetSeries, (t, b) => isIncome ? t - b : b - t));
                serieses.AddRange(series.ToEChartsSeries());
            }
        }

        return new GetCategoryChartResult([.. serieses]);
    }
}
