namespace FinanceManager.Infrastructure.Repositories;

using EFCore.BulkExtensions;
using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Entities;
using FinanceManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public sealed class BankRecordRepository(FinanceManagerDbContext dbContext) : IBankRecordRepository
{
    public async Task<BankRecord> GetByIdAsync(Guid id)
    {
        var bankRecord = await dbContext.BankRecords
            .Include(br => br.BankAccount)
            .Include(br => br.Transactions)
            .FirstOrDefaultAsync(br => br.Id == id);

        if (bankRecord is not null)
        {
            return bankRecord;
        }

        throw new KeyNotFoundException($"BankRecord with Id {id} not found.");
    }

    public async Task<IEnumerable<BankRecord>> GetByIdsAsync(IEnumerable<Guid> ids)
    {
        var idSet = ids as HashSet<Guid> ?? [.. ids];
        var bankRecords = await dbContext.BankRecords
            .Where(br => idSet.Contains(br.Id))
            .ToListAsync();

        if (bankRecords.Count == idSet.Count)
        {
            return bankRecords;
        }

        throw new KeyNotFoundException("One or more BankRecords not found for the provided Ids.");
    }

    public async Task<IEnumerable<BankRecord>> FilterDuplicatesAsync(IEnumerable<BankRecord> bankRecords)
    {
        var records = bankRecords as List<BankRecord> ?? [.. bankRecords];
        if (records.Count == 0) return [];

        var recordKeys = records.Select(r => new BankRecordKey(r.Amount, r.Date, r.BankAccount.Bank, r.BankAccount.AccountNumber, r.Description, r.Type, r.Reference)).ToHashSet();

        var dates = recordKeys.Select(k => k.Date).Distinct().ToList();
        var amounts = recordKeys.Select(k => k.Amount).Distinct().ToList();

        var candidateKeys = await dbContext.BankRecords
            .AsNoTracking()
            .Where(br => dates.Contains(br.Date) && amounts.Contains(br.Amount))
            .Select(br => new BankRecordKey(br.Amount, br.Date, br.BankAccount.Bank, br.BankAccount.AccountNumber, br.Description, br.Type, br.Reference))
            .ToListAsync();

        var keySet = candidateKeys.ToHashSet();
        return records.Where(r => keySet.Contains(new BankRecordKey(r.Amount, r.Date, r.BankAccount.Bank, r.BankAccount.AccountNumber, r.Description, r.Type, r.Reference)));
    }

    public Task CreateAsync(IEnumerable<BankRecord> bankRecords)
    {
        dbContext.BulkInsertAsync(bankRecords);
        return Task.CompletedTask;
    }

    private readonly record struct BankRecordKey(decimal Amount, DateTime Date, string Bank, string? AccountNumber, string Description, string? Type, string? Reference);
}
