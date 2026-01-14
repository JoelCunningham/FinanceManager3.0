using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Entities;

namespace FinanceManager.Application.Services
{
    public class TransactionService(ITransactionRepository transactionRepository)
    {
        public IEnumerable<Transaction> SearchTransactions(string searchTerm)
        {
            return [];
        }
    }
}
