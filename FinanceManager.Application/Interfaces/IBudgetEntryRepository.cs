namespace FinanceManager.Application.Interfaces;

using FinanceManager.Domain.Entities;

public interface IBudgetEntryRepository
{
    Task<IEnumerable<BudgetEntry>> GetByRangeAsync(DateOnly startDate, DateOnly endDate, IEnumerable<Guid> categoryIds);

    Task<BudgetEntry?> GetByIdAsync(Guid id);
    Task CreateAsync(BudgetEntry entry);
    Task UpdateAsync(BudgetEntry entry);
    Task DeleteAsync(Guid id);
}