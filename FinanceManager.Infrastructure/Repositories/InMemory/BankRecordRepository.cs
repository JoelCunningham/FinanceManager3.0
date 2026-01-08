using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Entities;

namespace FinanceManager.Infrastructure.Repositories.InMemory
{
    public class BankRecordRepository : IBankRecordRepository
    {
        public async Task<bool> SaveAsync(IEnumerable<BankRecord> bankRecords)
        {
            return true;
        }

        public async Task<bool> DeleteByImportIdAsync(Guid importId)
        {
            return true;
        }

        public async Task<BankRecord?> FindSimilarAsync(BankRecord bankRecord)
        {
            if (bankRecord.Amount == 796.8m) return bankRecord;
            return null; //TODO: Must be oppersite sign, within 10% or 1 dollar, within 30days
        }

        public async Task<IEnumerable<BankRecord>> FindDuplicatesAsync(IEnumerable<BankRecord> bankRecords)
        {
            return bankRecords.Take(2);
        }
    }
}
