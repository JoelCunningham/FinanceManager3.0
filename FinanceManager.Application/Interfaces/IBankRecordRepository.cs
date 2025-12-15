using FinanceManager.Domain.Entities;

namespace FinanceManager.Application.Interfaces
{
    public interface IBankRecordRepository
    {
        Task<bool> SaveBankRecordsAsync(IEnumerable<BankRecord> bankRecords);
    }
}
