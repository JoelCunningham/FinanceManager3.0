namespace FinanceManager.Application.Interfaces;

using FinanceManager.Domain.Entities;
public interface ICategoryGroupRepository
{
    Task<IEnumerable<CategoryGroup>> GetAllAsync();
    Task<CategoryGroup?> GetOrDefaultAsync(Guid id);
    Task CreateAsync(CategoryGroup categoryGroup);
    Task UpdateAsync(CategoryGroup categoryGroup);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsWithNameAsync(string name);
}