using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Entities;

namespace FinanceManager.Application.Services
{
    public class TransactionService(ITransactionRepository transactionRepository)
    {
        private readonly ITransactionRepository transactionRepository = transactionRepository;

        public async Task<IEnumerable<Transaction>> GetTransactionsByNameAsync(string name = "")
        {
            return await transactionRepository.GetTransactionsByNameAsync(name);
        }
    }
}
