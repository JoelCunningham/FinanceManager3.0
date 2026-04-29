using FinanceManager.Application.DTOs;
using FinanceManager.Domain.Entities;

namespace FinanceManager.Application.Interfaces
{
    public interface ITransactionRepository
    {
        Task<Transaction> GetByIdAsync(Guid id);
        Task<PagedResult<Transaction>> GetPagedAsync(FilterQuery query);
        Task CreateAsync(IEnumerable<Transaction> transactions);
        Task CreateOrUpdateAsync(Transaction transaction);
        Task UpdateAsync(Transaction transaction);
        Task DeleteAsync(Guid id);
        Task DeleteOrSkipAsync(Guid id);
    }
}
