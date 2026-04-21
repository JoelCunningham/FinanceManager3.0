namespace FinanceManager.Application.UseCases.Transactions;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Enums;
using FinanceManager.Application.UseCases.Categories;
using FinanceManager.Application.Utilities;

public sealed record GetChart2DataResult(
    List<TransactionSummary> Transactions,
    List<CategorySummary> RelevantCategories,
    IReadOnlyDictionary<string, decimal> BudgetTotals,
    bool HasLargerScopedBudgets
) : UseCaseResult;

public sealed class GetChart2Data(TransactionHelper transactionHelper, GetBudgetScopes getBudgetScopes, GetCategoryList getCategoryList)
{
    public async Task<GetChart2DataResult> ExecuteAsync(ScopedPeriod period, Guid? drilldownGroupId, TransactionsGraphMode mode)
    {
        var query = new FilterQuery
        {
            FilterDateFrom = period.StartDate.ToDateTime(TimeOnly.MinValue),
            FilterDateTo = period.EndDate.ToDateTime(TimeOnly.MaxValue),
            FilterStatus = ReviewStatus.Reviewed
        };

        var transactions = await transactionHelper.GetTransactionsForRange(query);
        var categories = (await getCategoryList.ExecuteAsync()).Categories;

        var relevantCategories = mode == TransactionsGraphMode.Income
            ? categories.Where(c => c.IsIncome).ToList()
            : categories.Where(c => !c.IsIncome).ToList();

        if (drilldownGroupId is Guid id)
        {
            relevantCategories = [.. relevantCategories.Where(c => c.GroupId == id)];
        }

        var budgetTotals = await transactionHelper.GetBudgetPerLabel(period.StartDate, period.EndDate, relevantCategories, drilldownGroupId is not null);
        var scopes = await getBudgetScopes.ExecuteAsync(period);

        var hasLargerScopedBudgets = scopes.GreatestScopeInPeriod > period.Scope;

        return new GetChart2DataResult(transactions, relevantCategories, budgetTotals, hasLargerScopedBudgets);
    }
}
