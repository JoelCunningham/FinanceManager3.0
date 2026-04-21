namespace FinanceManager.Application.UseCases;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Enums;
using FinanceManager.Application.UseCases.Transactions;

public sealed class TransactionsWorkflow(GetBudgetScopes getBudgetScopes, GetChart1Data getChart1Data, GetChart2Data getChart2Data, GetCategoryGroupList getCategoryGroupList)
{
    public Task<GetBudgetScopesResult> GetBudgetScopesAsync(ScopedRange range) => getBudgetScopes.ExecuteAsync(range);
    public Task<GetChart1DataResult> GetChart1DataAsync(ScopedRange range, Guid? drilldownGroupId, TransactionsGraphMode mode) => getChart1Data.ExecuteAsync(range, drilldownGroupId, mode);
    public Task<GetChart2DataResult> GetChart2DataAsync(ScopedRange range, Guid? drilldownGroupId, TransactionsGraphMode mode) => getChart2Data.ExecuteAsync(range, drilldownGroupId, mode);
    public Task<GetCategoryGroupsResult> GetCategoryGroupsAsync() => getCategoryGroupList.ExecuteAsync();
}
