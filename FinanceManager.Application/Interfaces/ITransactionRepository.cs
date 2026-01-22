using FinanceManager.Domain.Entities;

namespace FinanceManager.Application.Interfaces
{
    public interface ITransactionRepository
    {
        Task SaveAsync(IEnumerable<Transaction> transactions);
        Task<IEnumerable<Transaction>> GetUncategorised();
    }
}
