namespace FinanceManager.Application.Services;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Entities;
using FinanceManager.Domain.Enums;

public class BudgetService(IBudgetEntryRepository BudgetEntryRepository, IBudgetPeriodRepository BudgetPeriodRepository)
{
    public async Task<IEnumerable<BudgetEntry>> GetBudget(ScopedPeriod period, IEnumerable<Category> categories)
    {
        return await BudgetEntryRepository.GetByRangeAsync(period.StartDate, period.EndDate, categories);
    }

    public async Task<Scope?> GetCurrentScope()
    {
        var currentPeriod = await BudgetPeriodRepository.GetCurrentAsync();
        return currentPeriod?.Scope;
    }

    public async Task<Scope> GetGreatestScopeInPeriod(ScopedPeriod period)
    {
        var periods = await BudgetPeriodRepository.GetByRangeAsync(period.StartDate, period.EndDate);
        return periods.Max(p => p.Scope);
    }
}