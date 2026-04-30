using FinanceManager.Application.DTOs;
using FinanceManager.Domain.Entities;

namespace FinanceManager.Application.Interfaces
{
    public interface ITransactionRepository
    {
        Task<Transaction> GetByIdAsync(Guid id);
        Task<Transaction?> GetOrDefaultAsync(Guid id);
        Task<PagedResult<Transaction>> GetPagedTransactionsAsync(FilterQuery query);
        Task<PagedResult<Transaction>> GetPagedReimbursementsAsync(FilterQuery query);
        Task CreateAsync(Transaction transaction);
        Task CreateAsync(IEnumerable<Transaction> transactions);
        Task CreateOrUpdateAsync(Transaction transaction);
        Task UpdateAsync(Transaction transaction);
        Task DeleteAsync(Guid id);
        Task DeleteOrSkipAsync(Guid id);
    }
}
