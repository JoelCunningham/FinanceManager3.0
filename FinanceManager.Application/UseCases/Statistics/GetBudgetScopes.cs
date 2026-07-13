namespace FinanceManager.Application.UseCases.Statistics;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Interfaces;
using FinanceManager.Application.UseCases;
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
