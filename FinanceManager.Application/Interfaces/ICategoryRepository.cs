namespace FinanceManager.Application.Interfaces;

using FinanceManager.Domain.Entities;

public interface ICategoryRepository
{
    Task<IEnumerable<Category>> GetAllAsync();
    Task<Category?> GetOrDefaultAsync(Guid id);
    Task CreateAsync(Category category);
    Task UpdateAsync(Category category);
}