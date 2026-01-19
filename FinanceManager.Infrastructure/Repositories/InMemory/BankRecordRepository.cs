using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Entities;

namespace FinanceManager.Infrastructure.Repositories.InMemory
{
    public class BankRecordRepository : IBankRecordRepository
    {
        private readonly List<BankRecord> _bankRecords = [];

        public async Task<bool> SaveAsync(IEnumerable<BankRecord> bankRecords)
        {
            _bankRecords.AddRange(bankRecords);
            return true;
        }

        public async Task<IEnumerable<BankRecord>> FindDuplicatesAsync(IEnumerable<BankRecord> bankRecords)
        {
            var duplicates = new List<BankRecord>();

            foreach (var bankRecord in bankRecords) {
                if (_bankRecords.Any(br => 
                    br.Bank == bankRecord.Bank &&
                    br.AccountNumber == bankRecord.AccountNumber &&
                    br.Amount == bankRecord.Amount &&
                    br.Date == bankRecord.Date &&
                    br.Description == bankRecord.Description &&
                    br.Type == bankRecord.Type &&
                    br.Reference == bankRecord.Reference &&
                    br.Id != bankRecord.Id
                ))
                {
                    duplicates.Add(bankRecord);
                }
            }

            return duplicates;
        }

        public async Task<IEnumerable<BankRecord>> GetByIdsAsync(IEnumerable<Guid> ids)
        {
            return [.. _bankRecords.Where(br => ids.Contains(br.Id))];
        }
    }
}
