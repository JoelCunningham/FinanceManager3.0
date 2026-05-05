using FinanceManager.Domain.Entities;

namespace FinanceManager.Application.Interfaces;

public interface IBankAccountRepository
{
    Task<IEnumerable<BankAccount>> GetAllAsync();
    Task CreateAsync(BankAccount bankAccount);
    Task<IEnumerable<BankAccount>> GetOrCreateAsync(string Bank, IEnumerable<string?> AccountNumbers);
}
