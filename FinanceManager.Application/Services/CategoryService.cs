using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Entities;

namespace FinanceManager.Application.Services
{
    public class CategoryService(ICategoryRepository categoryRepository)
    {
        public async Task<IEnumerable<Category>> GetCategories()
        {
            var categories = await categoryRepository.GetAllAsync();
            return categories.OrderBy(c => c.Group.Name).ThenBy(c => c.Name);
        }

        public async Task<IEnumerable<Category>> GetIncomeCategories()
        {
            return (await GetCategories()).Where(c => c.Group.IsIncome);
        }

        public async Task<IEnumerable<Category>> GetExpenseCategories()
        {
            return (await GetCategories()).Where(c => !c.Group.IsIncome);
        }
    }
}
