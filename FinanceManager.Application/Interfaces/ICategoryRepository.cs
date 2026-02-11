using FinanceManager.Domain.Entities;

namespace FinanceManager.Application.Interfaces
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<Category>> GetAllAsync();
    }
}
