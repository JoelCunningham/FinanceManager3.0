using FinanceManager.Domain.Entities;

namespace FinanceManager.Application.Interfaces
{
    public interface ITransactionRepository
    {
        Task SaveTransactions(IEnumerable<Transaction> transactions);

        Task SearchTransactions(string searchTerm);
    }
}
