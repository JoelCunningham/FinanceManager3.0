namespace FinanceManager.Application.UseCases.Transactions;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Interfaces;
using FinanceManager.Application.Services;
using FinanceManager.Domain.Enums;

public sealed record GetBudgetScopesResult(BudgetScope CurrentScope, BudgetScope GreatestScopeInPeriod) : UseCaseResult;

public sealed class GetBudgetScopes(IBudgetPeriodRepository budgetPeriodRepository)
{
    public async Task<GetBudgetScopesResult> ExecuteAsync(ScopedPeriod period)
    {
        var currentScope = await GetCurrentScope() ?? BudgetScope.Monthly;
        var greatestScope = await GetGreatestScopeInPeriod(period);
        return new GetBudgetScopesResult(currentScope, greatestScope);
    }

    private async Task<BudgetScope?> GetCurrentScope()
    {
        var currentPeriod = await budgetPeriodRepository.GetCurrentAsync();
        return currentPeriod?.Scope;
    }

    private async Task<BudgetScope> GetGreatestScopeInPeriod(ScopedPeriod period)
    {
        var periods = await budgetPeriodRepository.GetByRangeAsync(period.StartDate, period.EndDate);
        if (!periods.Any()) return period.Scope;
        return periods.Max(p => p.Scope);
    }
}
