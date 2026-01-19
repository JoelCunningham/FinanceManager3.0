using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Entities;

namespace FinanceManager.Application.Services
{
    public class TransactionService(ITransactionRepository transactionRepository)
    {
        public async Task<IEnumerable<Transaction>> GetUncategorisedAsync()
        {
            return await transactionRepository.GetUncategorised();
        }
    }
}
