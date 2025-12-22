using FinanceManager.Application.DTOs;
using FinanceManager.Domain.Entities;

namespace FinanceManager.Application.Interfaces
{
    public interface IBankRecordRepository
    {
        Task<bool> SaveAsync(IEnumerable<BankRecord> bankRecords);
        Task<IEnumerable<BankRecord>> FindDuplicatesAsync(IEnumerable<BankRecord> bankRecords);
        Task<IEnumerable<BankRecord>> FindSimilarAsync(IEnumerable<BankRecord> bankRecords);
    }
}
