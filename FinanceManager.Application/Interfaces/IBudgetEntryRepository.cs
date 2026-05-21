namespace FinanceManager.Application.Interfaces;

using FinanceManager.Domain.Entities;
using FinanceManager.Domain.Enums;

public interface IBudgetEntryRepository
{
    Task<IEnumerable<BudgetEntry>> GetByRangeAsync(DateOnly startDate, DateOnly endDate, IEnumerable<Guid> categoryIds);
    Task<IEnumerable<BudgetEntry>> GetByBudgetYearAsync(Guid budgetYearId);
    Task<BudgetEntry?> GetByIdAsync(Guid id);
    Task CreateAsync(BudgetEntry entry);
    Task UpdateAsync(BudgetEntry entry);
    Task DeleteAsync(Guid id);
    Task StretchEntriesToScope(int year, BudgetScope oldScope, BudgetScope newScope);
}