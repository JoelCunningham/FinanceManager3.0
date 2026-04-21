namespace FinanceManager.Application.Interfaces;

using FinanceManager.Domain.Entities;
using FinanceManager.Domain.Enums;

public interface IBudgetYearRepository
{
    Task<BudgetYear?> GetCurrentAsync();
    Task<BudgetYear?> GetByYearAsync(int year);
    Task<IEnumerable<BudgetYear>> GetByRangeAsync(DateOnly startDate, DateOnly endDate);
    Task CreateAsync(int year, BudgetScope scope);
}