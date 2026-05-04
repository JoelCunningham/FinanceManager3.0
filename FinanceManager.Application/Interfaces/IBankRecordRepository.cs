using FinanceManager.Domain.Entities;

namespace FinanceManager.Application.Interfaces
{
    public interface IBankRecordRepository
    {
        Task<BankRecord> GetByIdAsync(Guid id);
        Task<IEnumerable<BankRecord>> GetByIdsAsync(IEnumerable<Guid> importId);
        Task<IEnumerable<BankRecord>> FilterDuplicatesAsync(IEnumerable<BankRecord> bankRecords);
        Task CreateAsync(IEnumerable<BankRecord> bankRecords);
    }
}
