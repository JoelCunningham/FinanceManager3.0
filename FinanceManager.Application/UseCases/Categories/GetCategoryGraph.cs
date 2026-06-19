namespace FinanceManager.Application.UseCases.Categories;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Enums;
using FinanceManager.Application.UseCases;
using FinanceManager.Application.Utilities;

public sealed record GetCategoryGraphResult(
    IReadOnlyDictionary<CategorySummary, IReadOnlyList<decimal>> ActualSeries,
    IReadOnlyDictionary<CategorySummary, IReadOnlyList<decimal>> BudgetSeries
) : UseCaseResult;

public sealed class GetCategoryGraph(ChartHelper chartHelper)
{
    public async Task<GetCategoryGraphResult> ExecuteAsync(IEnumerable<CategorySummary> categories, ScopedRange range)
    {
        var transactions = new Dictionary<CategorySummary, IReadOnlyList<decimal>>();
        var budgetSeries = new Dictionary<CategorySummary, IReadOnlyList<decimal>>();

        foreach (var category in categories)
        {
            var query = new FilterQuery
            {
                FilterDateFrom = range.StartDate.ToDateTime(TimeOnly.MinValue),
                FilterDateTo = range.EndDate.ToDateTime(TimeOnly.MaxValue),
                FilterStatus = ReviewStatus.Reviewed,
                FilterCategory = category
            };
            transactions.Add(category, await chartHelper.GetTransactionsPerPeriod(query, range));
            budgetSeries.Add(category, await chartHelper.GetBudgetPerMonthForCategories(range, [category], false));
        }

        return new GetCategoryGraphResult(transactions, budgetSeries);
    }
}
