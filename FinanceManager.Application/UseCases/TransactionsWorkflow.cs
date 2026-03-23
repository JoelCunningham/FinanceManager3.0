namespace FinanceManager.Application.UseCases;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Enums;
using FinanceManager.Application.UseCases.Transactions;

public sealed class TransactionsWorkflow(GetCategoriesForTransactions getCategoriesForTransactions, GetBudgetScopes getBudgetScopes, GetChart1Data getChart1Data, GetChart2Data getChart2Data)
{
    public Task<GetCategoriesForTransactionsResult> GetCategoriesAsync() => getCategoriesForTransactions.ExecuteAsync();
    public Task<GetBudgetScopesResult> GetBudgetScopesAsync(ScopedPeriod period) => getBudgetScopes.ExecuteAsync(period);
    public Task<GetChart1DataResult> GetChart1DataAsync(ScopedPeriod period, Guid? drilldownGroupId, TransactionsGraphMode mode) => getChart1Data.ExecuteAsync(period, drilldownGroupId, mode);
    public Task<GetChart2DataResult> GetChart2DataAsync(ScopedPeriod period, Guid? drilldownGroupId, TransactionsGraphMode mode) => getChart2Data.ExecuteAsync(period, drilldownGroupId, mode);
}
