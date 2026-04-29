using FinanceManager.Domain.Entities;

namespace FinanceManager.Application.Interfaces;

public interface IBankAccountRepository
{
    Task<IEnumerable<BankAccount>> GetAllAsync();
    Task CreateAsync(BankAccount bankAccount);
    Task<IEnumerable<BankAccount>> GetOrCreateAsync(IEnumerable<(string Bank, string? AccountNumber)> accountDetails);
}
