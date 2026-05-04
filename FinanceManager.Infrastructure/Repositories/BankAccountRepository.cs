namespace FinanceManager.Infrastructure.Repositories.EfCore;

using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Entities;
using FinanceManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public sealed class BankAccountRepository(FinanceManagerDbContext dbContext) : IBankAccountRepository
{
    public async Task<IEnumerable<BankAccount>> GetAllAsync()
    {
        return await dbContext.BankAccounts.AsNoTracking().ToListAsync();
    }

    public async Task CreateAsync(BankAccount bankAccount)
    {
        dbContext.BankAccounts.Add(bankAccount);
        await dbContext.SaveChangesAsync();
    }


    public async Task<IEnumerable<BankAccount>> GetOrCreateAsync(IEnumerable<(string Bank, string? AccountNumber)> accountDetails)
    {
        var result = new List<BankAccount>();

        foreach (var (bank, accountNumber) in accountDetails)
        {
            var existingAccount = await dbContext.BankAccounts.FirstOrDefaultAsync(ba => ba.Bank == bank && ba.AccountNumber == accountNumber);
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
                dbContext.BankAccounts.Add(newAccount);
                result.Add(newAccount);
            }
        }

        await dbContext.SaveChangesAsync();
        return result;
    }
}
