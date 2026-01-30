using FinanceManager.Domain.Entities;

namespace FinanceManager.Application.Interfaces
{
    public interface ITransferRepository
    {
        Task<Transfer> GetByIdAsync(Guid id);
        Task<IEnumerable<Transfer>> GetAllAsync();
        Task CreateAsync(Transfer transfer);
        Task CreateAsync(IEnumerable<Transfer> transfers);
        Task DeleteAsync(Guid id);
    }
}
