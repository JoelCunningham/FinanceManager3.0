namespace FinanceManager.Application.Interfaces;

using FinanceManager.Domain.Entities;

public interface IBudgetPeriodRepository
{
    Task<BudgetScope?> GetCurrentAsync();
    Task<IEnumerable<BudgetScope>> GetByRangeAsync(DateOnly startDate, DateOnly endDate);
}