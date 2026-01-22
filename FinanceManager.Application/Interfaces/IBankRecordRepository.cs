using FinanceManager.Domain.Entities;

namespace FinanceManager.Application.Interfaces
{
    public interface IBankRecordRepository
    {
        Task SaveAsync(IEnumerable<BankRecord> bankRecords);
        Task<BankRecord> GetByIdAsync(Guid id);
        Task<IEnumerable<BankRecord>> GetByIdsAsync(IEnumerable<Guid> importId);
        Task<IEnumerable<BankRecord>> FindDuplicatesAsync(IEnumerable<BankRecord> bankRecords);
    }
}
