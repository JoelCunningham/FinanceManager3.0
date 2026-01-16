using FinanceManager.Application.DTOs;
using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Entities;

namespace FinanceManager.Application.Services
{
    public class TransactionImportService
    (
        TransactionParserService parserService,
        IBankRecordRepository bankRecordRepository,
        ITransactionRepository transactionRepository
    )
    {
        public async Task<IEnumerable<BankRecord>> ImportAsync(Stream file, string bank, string extension)
        {
            var importId = Guid.NewGuid();

            var parser = parserService.GetParser(bank, extension);
            var parsed = (await parser.ParseTransactionsFileAsync(file)).ToList();

            var records = parsed.Select(t => t.ToBankRecord(bank, importId)).ToList();

            var duplicates = (await bankRecordRepository.FindDuplicatesAsync(records)).ToList();
            
            return records.Except(duplicates);
        }


        public async Task<bool> SaveAsync(IEnumerable<BankRecord> importedTransactions)
        {
            var success = await bankRecordRepository.SaveAsync(importedTransactions);

            if (success)
            {
                success = await transactionRepository.SaveAsync(
                    importedTransactions.Select(BankRecordToTransaction)
                );
            }

            return success;
        }

        private static async Task MatchTransfersAsync(List<TransactionInfo> transactions)
        {
            var internalTransfers = transactions.Where(t => t.BankRecord.IsInternalTransfer).ToList();

            foreach (var transaction in internalTransfers)
            {
                if (transaction.Transfers != null) continue;

                var match = internalTransfers.FirstOrDefault(p =>
                    p != transaction &&
                    p.Transfers == null &&
                    p.Amount == -transaction.Amount &&
                    p.Date.Date == transaction.Date.Date);

                if (match != null)
                {
                    transaction.SetTransfers(match);
                }
            }
        }

        private static Transaction BankRecordToTransaction(BankRecord record)
        {
            return new Transaction
            {
                Id = Guid.NewGuid(),
                RecordId = record.Id,
                Date = record.Date,
                Amount = record.Amount,
                Description = record.Description,
                CategoryId = null
            };
        }
    }
}