using FinanceManager.Domain.Entities;

namespace FinanceManager.Application.Interfaces
{
    public interface ITransactionRepository
    {
        Task<bool> SaveAsync(IEnumerable<Transaction> transactions);
        Task<IEnumerable<Transaction>> GetUncategorised();

        Task<IEnumerable<Transaction>> GetTransfers();
    }
}
