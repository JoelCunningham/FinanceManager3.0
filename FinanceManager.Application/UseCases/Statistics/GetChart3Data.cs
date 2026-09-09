namespace FinanceManager.Application.UseCases.Statistics;

using FinanceManager.Application.Constants;
using FinanceManager.Application.DTOs;
using FinanceManager.Application.Enums;
using FinanceManager.Application.UseCases;
using FinanceManager.Application.UseCases.Categories;
using FinanceManager.Application.Utilities;

public sealed record GetChart3DataResult(ChartOptions OptionsA, string TitleA, ChartOptions OptionsB, string TitleB) : UseCaseResult;

public sealed class GetChart3Data(ChartHelper chartHelper, GetCategories getCategoryList)
{
    public async Task<GetChart3DataResult> ExecuteAsync(IEnumerable<ScopedPeriod> range, ChartMode mode)
    {
        ChartOptions options1 = new(hideLegend: mode == ChartMode.Variance, axisPointerType: AxisPointerType.Shadow);
        ChartOptions options2 = new(hideLegend: mode == ChartMode.Variance, axisPointerType: AxisPointerType.Shadow);

        var categories = (await getCategoryList.ExecuteAsync()).Categories;
        var relevantCategories = categories
            .Where(c => mode == ChartMode.Net || mode == ChartMode.Variance || c.IsIncome == (mode == ChartMode.Income))
            .ToList();

        var transactionSerieses = await chartHelper.GetTransactions(relevantCategories, range, mode == ChartMode.Expense, mode != ChartMode.Net && mode != ChartMode.Variance);
        var transactionSeries = ChartHelper.FlattenSerieses(transactionSerieses);
        var transactionsCumulative = ChartHelper.CumulativeSeries(transactionSeries);

        var budgetSerieses = await chartHelper.GetBudgets(relevantCategories, range, false, false, mode == ChartMode.Net || mode == ChartMode.Variance);
        var budgetSeries = ChartHelper.FlattenSerieses(budgetSerieses);
        var budgetCumulative = ChartHelper.CumulativeSeries(budgetSeries);

        if (mode != ChartMode.Variance)
        {
            options1.AddSeries(ChartSeries.LineSeries("Activities", mode == ChartMode.Expense ? ColourConstants.ExpenseColour : ColourConstants.IncomeColour, transactionSeries));
            options2.AddSeries(ChartSeries.LineSeries("Cumulative activities", mode == ChartMode.Expense ? ColourConstants.ExpenseColour : ColourConstants.IncomeColour, transactionsCumulative));

            options1.AddSeries(ChartSeries.LineSeries("Budget", mode == ChartMode.Expense ? ColourConstants.IncomeColour : ColourConstants.ExpenseColour, budgetSeries, true));
            options2.AddSeries(ChartSeries.LineSeries("Cumulative budget", mode == ChartMode.Expense ? ColourConstants.IncomeColour : ColourConstants.ExpenseColour, budgetCumulative, true));
        }
        else
        {
            options1.AddSeries(ChartSeries.LineSeries("Variance", ColourConstants.NeutralColour, transactionSeries.Zip(budgetSeries, (t, b) => t - b)));
            options2.AddSeries(ChartSeries.LineSeries("Cumulative variance", ColourConstants.NeutralColour, transactionsCumulative.Zip(budgetCumulative, (t, b) => t - b)));
        }

        options1.SetLabels(range.Select(m => m.PeriodDescription));
        options2.SetLabels(range.Select(m => m.PeriodDescription));

        string title(bool cumulative) => mode switch
        {
            ChartMode.Expense => $"{(cumulative ? "Cumulative expenses" : "Expenses")} over time",
            ChartMode.Income => $"{(cumulative ? "Cumulative income" : "Income")} over time",
            ChartMode.Net => $"{(cumulative ? "Cumulative net activity" : "Net activity")} over time",
            ChartMode.Variance => $"{(cumulative ? "Cumulative variance" : "Variance")} over time",
            _ => "Transactions"
        };

        return new GetChart3DataResult(options1, title(false), options2, title(true));
    }
}
