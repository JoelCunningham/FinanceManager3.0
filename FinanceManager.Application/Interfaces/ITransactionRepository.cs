using FinanceManager.Domain.Entities;

namespace FinanceManager.Application.Interfaces
{
    public interface ITransactionRepository
    {
        Task<Transaction> GetByIdAsync(Guid id);
        Task<IEnumerable<Transaction>> GetUnreviewedAsync();
        Task<IEnumerable<Transaction>> GetBySearchTermAsync(string searchTerm);
        Task CreateAsync(Transaction transaction);
        Task CreateAsync(IEnumerable<Transaction> transactions);
        Task CreateOrUpdateAsync(Transaction transaction);
        Task UpdateAsync(Transaction transaction);
        Task DeleteAsync(Guid id);
    }
}
