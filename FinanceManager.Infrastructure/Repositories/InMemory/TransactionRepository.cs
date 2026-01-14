using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Entities;

namespace FinanceManager.Infrastructure.Repositories.InMemory
{
    public class TransactionRepository : ITransactionRepository
    {
        public Task SaveTransactions(IEnumerable<Transaction> transactions, IEnumerable<TransactionLink> links)
        {
            return Task.CompletedTask;
        }

        public Task SearchTransactions(string searchTerm)
        {
            return Task.CompletedTask;
        }

    }
}
