using FinanceManager.Application.DTOs;
using FinanceManager.Domain.Entities;

namespace FinanceManager.Application.Interfaces
{
    public interface IBankRecordRepository
    {
        Task<bool> SaveAsync(IEnumerable<BankRecord> bankRecords);
        Task<bool> DeleteByImportIdAsync(Guid importId);
        Task<BankRecord?> FindSimilarAsync(BankRecord bankRecord);
        Task<IEnumerable<BankRecord>> FindDuplicatesAsync(IEnumerable<BankRecord> bankRecords);
    }
}
