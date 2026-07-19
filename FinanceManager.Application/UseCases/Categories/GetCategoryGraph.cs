namespace FinanceManager.Application.UseCases.Categories;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.UseCases;
using FinanceManager.Application.Utilities;

public sealed record GetCategoryGraphResult(
    IReadOnlyDictionary<CategorySummary, IReadOnlyList<decimal>> ActualSeries,
    IReadOnlyDictionary<CategorySummary, IReadOnlyList<decimal>> BudgetSeries
) : UseCaseResult;

public sealed class GetCategoryGraph(ChartHelper chartHelper)
{
    public async Task<GetCategoryGraphResult> ExecuteAsync(IEnumerable<CategorySummary> categories, IEnumerable<ScopedPeriod> periods)
    {
        var transactions = new Dictionary<CategorySummary, IReadOnlyList<decimal>>();
        var budgetSeries = new Dictionary<CategorySummary, IReadOnlyList<decimal>>();

        foreach (var category in categories)
        {
            transactions.Add(category, await chartHelper.GetTransactionsPerPeriod(category, periods));
            budgetSeries.Add(category, await chartHelper.GetBudgetsPerPeriod(category, periods));
        }

        return new GetCategoryGraphResult(transactions, budgetSeries);
    }
}
