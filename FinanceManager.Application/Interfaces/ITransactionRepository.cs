using FinanceManager.Application.DTOs;
using FinanceManager.Domain.Entities;

namespace FinanceManager.Application.Interfaces
{
    public interface ITransactionRepository
    {
        Task<Transaction> GetByIdAsync(Guid id);
        Task<IEnumerable<Transaction>> GetByRecordIdAsync(Guid recordId, Guid? excludeId = null);
        Task<IEnumerable<Transaction>> GetByCategoryIdAsync(Guid categoryId);
        Task<IEnumerable<Transaction>> GetReimbursementsAsync(IEnumerable<Guid> reimbursedTransactionIds);
        Task<IEnumerable<Transaction>> GetTransactionsAsync(FilterQuery query);
        Task<PagedResult<Transaction>> GetPagedTransactionsAsync(FilterQuery query);
        Task CreateAsync(IEnumerable<Transaction> transactions);
        Task CreateOrUpdateAsync(Transaction transaction);
        Task UpdateAsync(Transaction transaction);
        Task DeleteAsync(Guid id);
        Task DeleteOrSkipAsync(Guid id);
        Task<bool> HasTransactionsForCategoryGroupAsync(Guid categoryGroupId);
        Task<(DateOnly Min, DateOnly Max)> GetRangeAsync(Guid? categoryGroupId = null);
    }
}
