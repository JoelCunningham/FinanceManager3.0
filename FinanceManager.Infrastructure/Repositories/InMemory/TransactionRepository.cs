using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Entities;

namespace FinanceManager.Infrastructure.Repositories.InMemory
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly List<Transaction> _transactions = [];

        public async Task SaveAsync(IEnumerable<Transaction> transactions)
        {
            _transactions.AddRange(transactions);
            //throw here on failure
        }

        public async Task<IEnumerable<Transaction>> GetUncategorised()
        {
            return _transactions.Where(t => t.CategoryId == null);
            // TODO, change this to discard transfers
        }

        public async Task<IEnumerable<Transaction>> SearchAsync(string searchTerm)
        {
            return _transactions.Where(t => t.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
        }
    }
}
