namespace FinanceManager.Infrastructure.Repositories;

using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Entities;
using FinanceManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public sealed class BankAccountRepository(IFinanceManagerDbContextFactory dbContextFactory) : IBankAccountRepository
{
    public async Task<IEnumerable<BankAccount>> GetAllAsync()
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        return await dbContext.BankAccounts.AsNoTracking().ToListAsync();
    }

    public async Task CreateAsync(BankAccount bankAccount)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        await dbContext.BankAccounts.AddAsync(bankAccount);
        await dbContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<BankAccount>> GetOrCreateAsync(string bank, IEnumerable<string?> accountNumbers)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var requested = accountNumbers.Distinct().ToList();
        if (requested.Count == 0) return [];

        var existingAccounts = await dbContext.BankAccounts
            .Where(ba => ba.Bank == bank && requested.Contains(ba.AccountNumber))
            .ToListAsync();

        var newAccounts = new List<BankAccount>();
        var allAccounts = new List<BankAccount>(requested.Count);

        foreach (var accountNumber in requested)
        {
            if (existingAccounts.Any(a => a.AccountNumber == accountNumber))
            {
                allAccounts.Add(existingAccounts.First(a => a.AccountNumber == accountNumber));
                continue;
            }

            var newAccount = new BankAccount { Id = Guid.NewGuid(), Bank = bank, AccountNumber = accountNumber };

            allAccounts.Add(newAccount);
            newAccounts.Add(newAccount);
        }

        if (newAccounts.Count > 0) await dbContext.BulkInsertOwnedAsync(newAccounts);
        return allAccounts;
    }
}
