using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Entities;
using FinanceManager.Domain.Enums;

namespace FinanceManager.Infrastructure.Repositories.InMemory
{
    public class TransactionRepository : ITransactionRepository
    {
        private List<Transaction> _transactions;

        public TransactionRepository()
        {
            _transactions = [
                new() { Id = "1", Amount = 1.00M, Date = DateTime.Now, Description = "Parking", Type = TransactionType.Debit },
                new() { Id = "2", Amount = 2.00M, Date = DateTime.Now, Description = "Wage", Type = TransactionType.Credit },
                new() { Id = "3", Amount = 3.00M, Date = DateTime.Now, Description = "Drinks", Type = TransactionType.Reimbursement },
            ];
        }

        public async Task<IEnumerable<Transaction>> GetTransactionsByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return await Task.FromResult(_transactions);
            }

            return _transactions.Where(t => t.Description.Contains(name, StringComparison.OrdinalIgnoreCase));
        }
    }
}
