namespace FinanceManager.Application.Interfaces;

using FinanceManager.Domain.Entities;

public interface IBankAccountRepository
{
    Task<IEnumerable<BankAccount>> GetAllAsync();
    Task CreateAsync(BankAccount bankAccount);
    Task<IEnumerable<BankAccount>> GetOrCreateAsync(string Bank, IEnumerable<string?> AccountNumbers);
}
