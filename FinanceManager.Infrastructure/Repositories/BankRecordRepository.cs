namespace FinanceManager.Infrastructure.Repositories;

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
        var duplicates = new List<BankRecord>();

        foreach (var bankRecord in bankRecords)
        {
            var exists = await dbContext.BankRecords.AnyAsync(br =>
                br.BankAccountId == bankRecord.BankAccountId &&
                br.Amount == bankRecord.Amount &&
                br.Date == bankRecord.Date &&
                br.Description == bankRecord.Description &&
                br.Type == bankRecord.Type &&
                br.Reference == bankRecord.Reference &&
                br.Id != bankRecord.Id);

            if (exists)
            {
                duplicates.Add(bankRecord);
            }
        }

        return duplicates;
    }

    public Task CreateAsync(IEnumerable<BankRecord> bankRecords)
    {
        dbContext.BankRecords.AddRange(bankRecords);
        return Task.CompletedTask;
    }
}
