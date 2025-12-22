using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Entities;
using FinanceManager.Domain.Enums;

namespace FinanceManager.Infrastructure.Repositories.InMemory
{
    public class TransactionRepository : ITransactionRepository
    {
        public Task SaveTransactions(IEnumerable<Transaction> transactions, IEnumerable<TransactionLink> links)
        {
            return Task.CompletedTask;
        }
    }
}
