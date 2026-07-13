namespace FinanceManager.Application.UseCases.Statistics;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Enums;
using FinanceManager.Application.Interfaces;
using FinanceManager.Application.UseCases;
using FinanceManager.Application.UseCases.Categories;
using FinanceManager.Application.Utilities;

public sealed record GetChart2DataResult(
    List<TransactionSummary> Transactions,
    List<CategorySummary> RelevantCategories,
    IReadOnlyDictionary<string, decimal> BudgetTotals,
    bool HasLargerScopedBudgets
) : UseCaseResult;

public sealed class GetChart2Data(ChartHelper transactionHelper, GetBudgetScopes getBudgetScopes, GetCategories getCategoryList, ITransactionRepository transactionRepository)
{
    public async Task<GetChart2DataResult> ExecuteAsync(ScopedRange range, Guid? drilldownGroupId, TransactionsGraphMode mode)
    {
        var query = new FilterQuery
        {
            FilterDateFrom = range.StartDate.ToDateTime(TimeOnly.MinValue),
            FilterDateTo = range.EndDate.ToDateTime(TimeOnly.MaxValue),
            FilterStatus = ReviewStatus.Reviewed
        };

        var transactions = (await transactionRepository.GetTransactionsAsync(query)).Select(TransactionSummary.FromTransaction).ToList();
        var categories = (await getCategoryList.ExecuteAsync()).Categories;

        var relevantCategories = mode == TransactionsGraphMode.Income
            ? [.. categories.Where(c => c.IsIncome)]
            : categories.Where(c => !c.IsIncome).ToList();

        if (drilldownGroupId is Guid id)
        {
            relevantCategories = [.. relevantCategories.Where(c => c.GroupId == id)];
        }

        var budgetTotals = await transactionHelper.GetBudgetPerLabel(range.StartDate, range.EndDate, relevantCategories, drilldownGroupId is not null);
        var scopes = await getBudgetScopes.ExecuteAsync(range);

        var hasLargerScopedBudgets = scopes.GreatestScopeInRange > range.Scope;

        return new GetChart2DataResult(transactions, relevantCategories, budgetTotals, hasLargerScopedBudgets);
    }
}
