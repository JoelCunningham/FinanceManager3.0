using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Entities;
using System.Globalization;

namespace FinanceManager.Infrastructure.Repositories.InMemory
{
    public class BankRecordRepository : IBankRecordRepository
    {

        private readonly List<BankRecord> _bankRecords = new();

        public BankRecordRepository() { 
            // Duplicate 1
            _bankRecords.Add(new BankRecord
            {
                Id = Guid.NewGuid(),
                ImportId = Guid.NewGuid(),
                Bank = "Westpac",
                AccountNumber = "033092585544",
                Amount = 68.11m,
                Date = DateTime.Parse("28/11/2025", new CultureInfo("en-AU")),
                Description = "INTEREST PAID  (INCLUDES BONUS OF        $64.11)",
                Type = "INT",
                Reference = "",
                IsInternalTransfer = false
            });

            // Duplicate 2
            _bankRecords.Add(new BankRecord
            {
                Id = Guid.NewGuid(),
                ImportId = Guid.NewGuid(),
                Bank = "Westpac",
                AccountNumber = "033092585544",
                Amount = 1m,
                Date = DateTime.Parse("18/11/2025", new CultureInfo("en-AU")),
                Description = "DEPOSIT ONLINE 2400796 TFR Westpac Cho Interest payment",
                Type = "CREDIT",
                Reference = "",
                IsInternalTransfer = false
            });
        }

        public async Task<bool> SaveAsync(IEnumerable<BankRecord> bankRecords)
        {
            return true;
        }

        public async Task<bool> DeleteByImportIdAsync(Guid importId)
        {
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
    }
}
