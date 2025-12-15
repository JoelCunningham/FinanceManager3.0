using FinanceManager.Domain.Entities;

namespace FinanceManager.Application.Interfaces
{
    public interface ITransactionRepository
    {
        Task<IEnumerable<Transaction>> GetTransactionsByNameAsync(string name);
    }
}
