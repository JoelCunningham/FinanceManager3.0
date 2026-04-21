namespace FinanceManager.Application.UseCases.Transactions;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Enums;

public sealed record GetBudgetScopesResult(BudgetScope CurrentScope, BudgetScope GreatestScopeInRange) : UseCaseResult;

public sealed class GetBudgetScopes(IBudgetYearRepository budgetYearRepository)
{
    public async Task<GetBudgetScopesResult> ExecuteAsync(ScopedRange range)
    {
        var currentScope = await GetCurrentScope() ?? BudgetScope.Monthly;
        var greatestScope = await GetGreatestScopeInRange(range);
        return new GetBudgetScopesResult(currentScope, greatestScope);
    }

    private async Task<BudgetScope?> GetCurrentScope()
    {
        var currentBudgetYear = await budgetYearRepository.GetCurrentAsync();
        return currentBudgetYear?.Scope;
    }

    private async Task<BudgetScope> GetGreatestScopeInRange(ScopedRange range)
    {
        var budgetYears = await budgetYearRepository.GetByRangeAsync(range.StartDate, range.EndDate);
        if (!budgetYears.Any()) return range.Scope;
        return budgetYears.Max(p => p.Scope);
    }
}
