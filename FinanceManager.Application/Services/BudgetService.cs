namespace FinanceManager.Application.Services;

using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Entities;
using FinanceManager.Domain.Enums;

public class BudgetService(IBudgetEntryRepository BudgetEntryRepository, IBudgetPeriodRepository BudgetPeriodRepository)
{
    public async Task<IEnumerable<BudgetEntry>> GetBudget(DateOnly startDate, DateOnly endDate, IEnumerable<Category> categories)
    {
        return await BudgetEntryRepository.GetByRangeAsync(startDate, endDate, categories);
    }

    public async Task<Scope?> GetCurrentScope()
    {
        var currentPeriod = await BudgetPeriodRepository.GetCurrentAsync();
        if (currentPeriod is null)
        {
            return null;
        }
        else
        {
            return currentPeriod.Scope;
        }
    }

    public async Task<Scope> GetGreatestScopeInPeriod(DateOnly startDate, DateOnly endDate)
    {
        var periods = await BudgetPeriodRepository.GetByRangeAsync(startDate, endDate);
        return periods.Max(p => p.Scope);
    }
}