namespace FinanceManager.Application.UseCases.Statistics;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Interfaces;
using FinanceManager.Application.UseCases;
using FinanceManager.Domain.Enums;

public sealed record GetBudgetScopesResult(BudgetScope GreatestScopeInRange) : UseCaseResult;

public sealed class GetBudgetScopes(IBudgetYearRepository budgetYearRepository)
{
    public async Task<GetBudgetScopesResult> ExecuteAsync(IEnumerable<ScopedPeriod> range)
    {
        var budgetYears = await budgetYearRepository.GetByRangeAsync(range.First().StartDate, range.Last().EndDate);
        return new GetBudgetScopesResult(budgetYears.MaxBy(b => b.Scope)?.Scope ?? range.First().Scope);
    }
}
