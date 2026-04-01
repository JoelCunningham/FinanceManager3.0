namespace FinanceManager.Application.Interfaces;

using FinanceManager.Domain.Entities;
using FinanceManager.Domain.Enums;

public interface IBudgetPeriodRepository
{
    Task<BudgetPeriod?> GetCurrentAsync();
    Task<BudgetPeriod?> GetByYearAsync(int year);
    Task<IEnumerable<BudgetPeriod>> GetByRangeAsync(DateOnly startDate, DateOnly endDate);
    Task CreateAsync(int year, BudgetScope scope);
}