using FinanceManager.Domain.Entities;

namespace FinanceManager.Application.Services
{
    public class CategoryService()
    {
        public IEnumerable<Category> GetCategories()
        {
            return [new()
            {
                Id = Guid.NewGuid(),
                Name ="Fuel"
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name ="Groceries"
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name ="Phone"
            }];
        }
    }
}
