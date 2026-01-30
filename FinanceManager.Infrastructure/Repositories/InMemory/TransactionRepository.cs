using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Entities;

namespace FinanceManager.Infrastructure.Repositories.InMemory
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly List<Transaction> _transactions = [];

        public async Task<Transaction> GetByIdAsync(Guid id)
        {
            var transaction = _transactions.FirstOrDefault(t => t.Id == id);
            if (transaction == null)
            {
                throw new KeyNotFoundException($"Transaction with ID {id} not found.");
            }
            return transaction;
        }

        public async Task<IEnumerable<Transaction>> GetUnreviewedAsync()
        {
            return _transactions.Where(t => t.IsReviewed == false);
        }

        public async Task<IEnumerable<Transaction>> GetBySearchTermAsync(string searchTerm)
        {
            return _transactions.Where(t => t.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
        }

        public async Task CreateAsync(Transaction transaction)
        {
            _transactions.Add(transaction);
            //throw here on failure
        }

        public async Task CreateAsync(IEnumerable<Transaction> transactions)
        {
            _transactions.AddRange(transactions);
            //throw here on failure
        }

        public async Task CreateOrUpdateAsync(Transaction transaction)
        {
            var existingTransaction = _transactions.FirstOrDefault(t => t.Id == transaction.Id);
            if (existingTransaction != null)
            {
                _transactions.Remove(existingTransaction);
            }
            _transactions.Add(transaction);
        }

        public async Task UpdateAsync(Transaction transaction)
        {
            var existingTransaction = _transactions.FirstOrDefault(t => t.Id == transaction.Id);
            if (existingTransaction == null)
            {
                throw new KeyNotFoundException($"Transaction with ID {transaction.Id} not found.");
            }
            _transactions.Remove(existingTransaction);
            _transactions.Add(transaction);
        }

        public async Task DeleteAsync(Guid id)
        {
            var transaction = _transactions.FirstOrDefault(t => t.Id == id);
            if (transaction == null)
            {
                throw new KeyNotFoundException($"Transaction with ID {id} not found.");
            }
            _transactions.Remove(transaction);
        }
    }
}
