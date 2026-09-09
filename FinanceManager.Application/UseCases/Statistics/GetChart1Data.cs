namespace FinanceManager.Application.UseCases.Statistics;

using FinanceManager.Application.Constants;
using FinanceManager.Application.DTOs;
using FinanceManager.Application.Enums;
using FinanceManager.Application.UseCases;
using FinanceManager.Application.UseCases.Categories;
using FinanceManager.Application.Utilities;
using System.Linq;

public sealed record GetChart1DataResult(ChartOptions Options, string Title) : UseCaseResult;

public sealed class GetChart1Data(ChartHelper chartHelper, GetCategories getCategoryList)
{
    public async Task<GetChart1DataResult> ExecuteAsync(IEnumerable<ScopedPeriod> range, IEnumerable<CategoryGroupSummary> groups, Guid? drilldownGroupId, ChartMode mode)
    {
        ChartOptions options = new(axisPointerType: AxisPointerType.Shadow);

        var categories = (await getCategoryList.ExecuteAsync()).Categories;
        var relevantCategories = categories
            .Where(c => mode == ChartMode.IncomeAndExpense || c.IsIncome == (mode == ChartMode.Income))
            .Where(c => drilldownGroupId is null || c.GroupId == drilldownGroupId)
            .ToList();

        var transactionSerieses = await chartHelper.GetTransactions(relevantCategories, range, mode == ChartMode.Expense, mode != ChartMode.IncomeAndExpense, drilldownGroupId is null);
        foreach (var transactionSeries in transactionSerieses)
        {
            var name = drilldownGroupId is null
                ? categories.FirstOrDefault(c => c.GroupId == transactionSeries.Key)?.GroupName ?? CategoryConstants.UncategorisedName
                : categories.FirstOrDefault(c => c.Id == transactionSeries.Key)?.Name ?? CategoryConstants.UncategorisedName;

            var colour = drilldownGroupId is null
                ? categories.FirstOrDefault(c => c.GroupId == transactionSeries.Key)?.GroupColour ?? ColourConstants.UncategorisedColour
                : categories.FirstOrDefault(c => c.Id == transactionSeries.Key)?.Colour ?? ColourConstants.UncategorisedColour;

            var key = categories.FirstOrDefault(c => c.GroupId == transactionSeries.Key)?.GroupId;

            options.AddSeries(ChartSeries.BarSeries(name, colour, transactionSeries.Value.Select((v, i) => (v, key + "_" + i.ToString())).ToDictionary(x => x.Item2, x => x.v)));
        }

        var groupBudgetPrefix = drilldownGroupId is not null ? groups.FirstOrDefault(g => g.Id == drilldownGroupId)?.Name ?? CategoryConstants.UncategorisedName : null;

        if (mode == ChartMode.IncomeAndExpense)
        {
            var incomeBudgetSerieses = await chartHelper.GetBudgets(relevantCategories.Where(c => c.IsIncome), range, false);
            options.AddSeries(ChartSeries.LineSeries("Budget (Income)",ColourConstants.IncomeColour, ChartHelper.FlattenSerieses(incomeBudgetSerieses)));

            var expenseBudgetSerieses = await chartHelper.GetBudgets(relevantCategories.Where(c => !c.IsIncome), range, true);
            options.AddSeries(ChartSeries.LineSeries("Budget (Expense)", ColourConstants.ExpenseColour, ChartHelper.FlattenSerieses(expenseBudgetSerieses)));
        }
        else
        {
            var budgetName = groupBudgetPrefix is null ? "Budget" : $"{groupBudgetPrefix} Budget";
            var budgetSerieses = await chartHelper.GetBudgets(relevantCategories, range);
            options.AddSeries(ChartSeries.LineSeries(budgetName, mode == ChartMode.Income ? ColourConstants.IncomeColour : ColourConstants.ExpenseColour, ChartHelper.FlattenSerieses(budgetSerieses)));
        }

        options.SetLabels(range.Select(m => m.PeriodDescription));

        var title = (mode, drilldownGroupId is not null) switch
        {
            (ChartMode.Expense, false) => $"Expenses by area over time",
            (ChartMode.Income, false) => $"Income by area over time",
            (ChartMode.IncomeAndExpense, false) => $"Income and Expenses by area over time",
            (_, true) => $"{groups.FirstOrDefault(g => g.Id == drilldownGroupId)?.Name ?? "Group"} by category",
            _ => "Transactions"
        };

        return new GetChart1DataResult(options, title);
    }
}