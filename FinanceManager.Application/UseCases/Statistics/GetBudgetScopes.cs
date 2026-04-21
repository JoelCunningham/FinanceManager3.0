namespace FinanceManager.Application.UseCases.Transactions;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Enums;

public sealed record GetBudgetScopesResult(BudgetScope GreatestScopeInRange) : UseCaseResult;

public sealed class GetBudgetScopes(IBudgetYearRepository budgetYearRepository)
{
    public async Task<GetBudgetScopesResult> ExecuteAsync(ScopedRange range)
    {
        var budgetYears = await budgetYearRepository.GetByRangeAsync(range.StartDate, range.EndDate);
        return new GetBudgetScopesResult(budgetYears.MaxBy(b => b.Scope)?.Scope ?? range.Scope);
    }
}
