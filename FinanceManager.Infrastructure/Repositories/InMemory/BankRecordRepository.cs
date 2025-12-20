using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Entities;

namespace FinanceManager.Infrastructure.Repositories.InMemory
{
    public class BankRecordRepository : IBankRecordRepository
    {
        public async Task<bool> SaveBankRecordsAsync(IEnumerable<BankRecord> bankRecords)
        {
            return true;
        }

        public async Task<IEnumerable<BankRecord>> FindDuplicatesAsync(IEnumerable<BankRecord> bankRecords)
        {
            return bankRecords.Take(2);
        }
    }
}
