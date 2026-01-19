using FinanceManager.Domain.Entities;

namespace FinanceManager.Application.Interfaces
{
    public interface ITransferRepository
    {
        Task<bool> SaveAsync(IEnumerable<Transfer> transfers);
        Task<IEnumerable<Transfer>> GetAllAsync();
        Task<Transfer?> GetByIdAsync(Guid id);
        Task<bool> RemoveAsync(Guid id);
    }
}
