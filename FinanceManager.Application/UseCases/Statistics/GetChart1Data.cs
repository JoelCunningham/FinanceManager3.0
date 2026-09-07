namespace FinanceManager.Application.UseCases.Statistics;

using FinanceManager.Application.Constants;
using FinanceManager.Application.DTOs;
using FinanceManager.Application.Enums;
using FinanceManager.Application.UseCases;
using FinanceManager.Application.UseCases.Categories;
using FinanceManager.Application.Utilities;
using System.Linq;

public sealed record GetChart1DataResult(object[] Serieses, string[] Labels, string Title) : UseCaseResult;

public sealed class GetChart1Data(ChartHelper chartHelper, GetCategories getCategoryList)
{
    public async Task<GetChart1DataResult> ExecuteAsync(IEnumerable<ScopedPeriod> range, IEnumerable<CategoryGroupSummary> groups, Guid? drilldownGroupId, ChartMode mode)
    {
        var serieses = new List<object>();

        var categories = (await getCategoryList.ExecuteAsync()).Categories;
        var relevantCategories = categories
            .Where(c => mode == ChartMode.IncomeAndExpense || c.IsIncome == (mode == ChartMode.Income))
            .Where(c => drilldownGroupId is null || c.GroupId == drilldownGroupId)
            .ToList();

        var transactionSerieses = await chartHelper.GetTransactionsPerCategoryAndPeriod(relevantCategories, range, mode == ChartMode.Expense, mode != ChartMode.IncomeAndExpense, drilldownGroupId is null);
        foreach (var transactionSeries in transactionSerieses)
        {
            var name = drilldownGroupId is null
                ? categories.FirstOrDefault(c => c.GroupId == transactionSeries.Key)?.GroupName ?? CategoryConstants.UncategorisedName
                : categories.FirstOrDefault(c => c.Id == transactionSeries.Key)?.Name ?? CategoryConstants.UncategorisedName;

            var colour = drilldownGroupId is null
                ? categories.FirstOrDefault(c => c.GroupId == transactionSeries.Key)?.GroupColour ?? CategoryConstants.UncategorisedColour
                : categories.FirstOrDefault(c => c.Id == transactionSeries.Key)?.Colour ?? CategoryConstants.UncategorisedColour;

            var key = categories.FirstOrDefault(c => c.GroupId == transactionSeries.Key)?.GroupId;

            var series = ChartSeries.BarSeries(name, colour, transactionSeries.Value.Select((v, i) => (v, key + "_" + i.ToString())).ToDictionary(x => x.Item2, x => x.v));
            serieses.AddRange(series.ToEChartsSeries());
        }

        var groupBudgetPrefix = drilldownGroupId is not null ? groups.FirstOrDefault(g => g.Id == drilldownGroupId)?.Name ?? CategoryConstants.UncategorisedName : null;

        if (mode == ChartMode.IncomeAndExpense)
        {
            var incomeBudgetSerieses = await chartHelper.GetBudgetsPerCategoryAndPeriod(relevantCategories.Where(c => c.IsIncome), range, false);
            var incomeBudgetSeries = ChartSeries.LineSeries("Budget (Income)", "#15723f", [.. incomeBudgetSerieses.Values.SelectMany((values, _) => values.Select((value, index) => (value, index))).GroupBy(x => x.index).OrderBy(g => g.Key).Select(g => g.Sum(x => x.value))]);
            serieses.AddRange(incomeBudgetSeries.ToEChartsSeries());

            var expenseBudgetSerieses = await chartHelper.GetBudgetsPerCategoryAndPeriod(relevantCategories.Where(c => !c.IsIncome), range, true);
            var expenseBudgetSeries = ChartSeries.LineSeries("Budget (Expense)", "#a81e2e", [.. expenseBudgetSerieses.Values.SelectMany((values, _) => values.Select((value, index) => (value, index))).GroupBy(x => x.index).OrderBy(g => g.Key).Select(g => g.Sum(x => x.value))]);
            serieses.AddRange(expenseBudgetSeries.ToEChartsSeries());
        }
        else
        {
            var budgetName = groupBudgetPrefix is null ? "Budget" : $"{groupBudgetPrefix} Budget";
            var budgetSerieses = await chartHelper.GetBudgetsPerCategoryAndPeriod(relevantCategories, range);
            var budgetSeries = ChartSeries.LineSeries(budgetName, mode == ChartMode.Income ? "#15723f" : "#a81e2e", [.. budgetSerieses.Values.SelectMany((values, _) => values.Select((value, index) => (value, index))).GroupBy(x => x.index).OrderBy(g => g.Key).Select(g => g.Sum(x => x.value))]);
            serieses.AddRange(budgetSeries.ToEChartsSeries());
        }

        var labels = range.Select(m => m.PeriodDescription).ToArray();

        var title = (mode, drilldownGroupId is not null) switch
        {
            (ChartMode.Expense, false) => $"Expenses by area over time",
            (ChartMode.Income, false) => $"Income by area over time",
            (ChartMode.IncomeAndExpense, false) => $"Income and Expenses by area over time",
            (_, true) => $"{groups.FirstOrDefault(g => g.Id == drilldownGroupId)?.Name ?? "Group"} by category",
            _ => "Transactions"
        };

        return new GetChart1DataResult([.. serieses], labels, title);
    }
}