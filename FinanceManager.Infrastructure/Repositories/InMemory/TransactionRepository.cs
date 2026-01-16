using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Entities;

namespace FinanceManager.Infrastructure.Repositories.InMemory
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly List<Transaction> _transactions = [];

        public async Task<bool> SaveAsync(IEnumerable<Transaction> transactions)
        {
            _transactions.AddRange(transactions);
            return true;
        }

        public async Task<IEnumerable<Transaction>> GetUncategorised()
        {
            return _transactions.Where(t => t.CategoryId == null);
            // TODO, change this to discard transfers
        }

        public async Task<IEnumerable<Transaction>> GetTransfers()
        {
            return _transactions.Where(t => t.Description.Contains(" TFR "));
            //TODO, change this to use relationships 
        }

    }
}
