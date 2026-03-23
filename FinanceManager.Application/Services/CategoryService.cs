namespace FinanceManager.Application.Services;

using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Entities;

public class CategoryService(ICategoryRepository categoryRepository)
{
    public async Task<IReadOnlyList<Category>> GetCategoriesAsync()
    {
        var categories = await categoryRepository.GetAllAsync();
        return [.. categories.OrderBy(c => c.Group.Name).ThenBy(c => c.Name)];
    }
}