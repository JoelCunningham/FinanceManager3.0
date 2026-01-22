using FinanceManager.Domain.Entities;

namespace FinanceManager.Application.Interfaces
{
    public interface ITransferRepository
    {
        Task SaveAsync(IEnumerable<Transfer> transfers);
        Task<IEnumerable<Transfer>> GetAllAsync();
        Task<Transfer> GetByIdAsync(Guid id);
        Task RemoveAsync(Guid id);
    }
}
