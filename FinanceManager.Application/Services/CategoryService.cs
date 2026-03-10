namespace FinanceManager.Application.Services;

using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Entities;

public class CategoryService(ICategoryRepository categoryRepository)
{
    public async Task<IReadOnlyList<CategoryGroup>> GetCategoryGroupsAsync()
    {
        var groups = await categoryRepository.GetAllGroupsAsync();
        return [.. groups.OrderBy(g => g.IsIncome ? 0 : 1).ThenBy(g => g.Name)];
    }

    public async Task<IReadOnlyList<Category>> GetCategoriesAsync()
    {
        var categories = await categoryRepository.GetAllAsync();
        return [.. categories.OrderBy(c => c.Group.Name).ThenBy(c => c.Name)];
    }
}