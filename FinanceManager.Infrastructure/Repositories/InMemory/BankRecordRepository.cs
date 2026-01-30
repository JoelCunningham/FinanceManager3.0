using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Entities;

namespace FinanceManager.Infrastructure.Repositories.InMemory
{
    public class BankRecordRepository : IBankRecordRepository
    {
        private readonly List<BankRecord> _bankRecords = [];

        public async Task<BankRecord> GetByIdAsync(Guid id)
        {
            var bankRecord = _bankRecords.FirstOrDefault(br => br.Id == id);
            if (bankRecord is not null)
            {
                return bankRecord;
            }
            throw new KeyNotFoundException($"BankRecord with Id {id} not found.");
        }

        public async Task<IEnumerable<BankRecord>> GetByIdsAsync(IEnumerable<Guid> ids)
        {
            var idSet = ids as HashSet<Guid> ?? [.. ids];
            var bankRecords = _bankRecords.Where(br => idSet.Contains(br.Id));

            if (bankRecords.ToList().Count == idSet.Count)
            {
                return bankRecords;
            }
            throw new KeyNotFoundException("One or more BankRecords not found for the provided Ids.");
        }

        public async Task<IEnumerable<BankRecord>> GetDuplicatesAsync(IEnumerable<BankRecord> bankRecords)
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
            //throw here on failure
        }

        public async Task CreateAsync(IEnumerable<BankRecord> bankRecords)
        {
            _bankRecords.AddRange(bankRecords);
            //throw here on failure
        }
    }
}
