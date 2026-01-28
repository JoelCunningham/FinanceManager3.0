using FinanceManager.Domain.Entities;

namespace FinanceManager.Application.Interfaces
{
    public interface ITransactionRepository
    {
        Task SaveAsync(IEnumerable<Transaction> transactions);
        Task<IEnumerable<Transaction>> GetUncategorised();
        Task<IEnumerable<Transaction>> SearchAsync(string searchTerm);
    }
}
