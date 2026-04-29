namespace FinanceManager.Infrastructure.Repositories.InMemory;

using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Entities;

public class BankAccountRepository : IBankAccountRepository
{
    private readonly List<BankAccount> _bankAccounts = [];

    public async Task<IEnumerable<BankAccount>> GetAllAsync()
    {
        return _bankAccounts;
    }
    
    public async Task CreateAsync(BankAccount bankAccount)
    {
        _bankAccounts.Add(bankAccount);
        //throw here on failure
    }

    public async Task<IEnumerable<BankAccount>> GetOrCreateAsync(IEnumerable<(string Bank, string? AccountNumber)> accountDetails)
    {
        var result = new List<BankAccount>();

        foreach (var (bank, accountNumber) in accountDetails)
        {
            var existingAccount = _bankAccounts.FirstOrDefault(ba => ba.Bank == bank && ba.AccountNumber == accountNumber);
            if (existingAccount is not null)
            {
                result.Add(existingAccount);
            }
            else
            {
                var newAccount = new BankAccount
                {
                    Id = Guid.NewGuid(),
                    Bank = bank,
                    AccountNumber = accountNumber
                };
                _bankAccounts.Add(newAccount);
                result.Add(newAccount);
            }
        }

        return result;
        //throw here on failure
    }
}
