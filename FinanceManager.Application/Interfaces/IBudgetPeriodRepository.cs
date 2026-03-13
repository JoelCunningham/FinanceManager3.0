namespace FinanceManager.Application.Interfaces;

using FinanceManager.Domain.Entities;

public interface IBudgetPeriodRepository
{
    Task<BudgetPeriod?> GetCurrentAsync();
    Task<IEnumerable<BudgetPeriod>> GetByRangeAsync(DateOnly startDate, DateOnly endDate);
}