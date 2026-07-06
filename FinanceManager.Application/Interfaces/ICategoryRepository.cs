namespace FinanceManager.Application.Interfaces;

using FinanceManager.Domain.Entities;

public interface ICategoryRepository
{
    Task<IEnumerable<Category>> GetAllAsync();
    Task<IEnumerable<Category>> GetByGroupIdAsync(Guid groupId);
    Task<Category?> GetOrDefaultAsync(Guid id);
    Task CreateAsync(Category category);
    Task UpdateAsync(Category category);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsWithNameAsync(string name, Guid groupId);
}