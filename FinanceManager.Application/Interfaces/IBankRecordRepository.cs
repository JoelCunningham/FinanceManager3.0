namespace FinanceManager.Application.Interfaces;

using FinanceManager.Domain.Entities;

public interface IBankRecordRepository
{
    Task<IEnumerable<BankRecord>> GetAllAsync();
    Task<BankRecord> GetByIdAsync(Guid id);
    Task<IEnumerable<BankRecord>> GetByIdsAsync(IEnumerable<Guid> importId);
    Task<IEnumerable<BankRecord>> FilterDuplicatesAsync(IEnumerable<BankRecord> bankRecords);
    Task<DateTime?> GetLatestDateAsync();
    Task CreateAsync(IEnumerable<BankRecord> bankRecords);
}
