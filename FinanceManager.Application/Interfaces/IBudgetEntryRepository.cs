namespace FinanceManager.Application.Interfaces;

using FinanceManager.Domain.Entities;

public interface IBudgetEntryRepository
{
    Task<IEnumerable<BudgetEntry>> GetByRangeAsync(DateOnly startDate, DateOnly endDate, IEnumerable<Guid> categoryIds);
}