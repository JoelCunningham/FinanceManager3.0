using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Entities;

namespace FinanceManager.Application.Services
{
    public class TransactionImportService
    (
        TransactionParserService parserService,
        IBankRecordRepository bankRecordRepository,
        ITransactionRepository transactionRepository,
        ITransferRepository transferRepository
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
                var transactions = importedTransactions.Select(BankRecordToTransaction).ToList();
                var transfers = BankRecordsToTransfers(importedTransactions).ToList();

                success = await transferRepository.SaveAsync(transfers);
                success &= await transactionRepository.SaveAsync(transactions);
                //TODO add rollbacks
            }

            return success;
        }

        private static IEnumerable<Transfer> BankRecordsToTransfers(IEnumerable<BankRecord> records)
        {
            List<Transfer> transfers = [];
            var transferTransactions = records.Where(t => t.IsInternalTransfer).ToList();

            foreach (var transfer in transferTransactions)
            {
                if (transfer.Amount < 0) continue;

                var match = FindTransferMatch(transfer, transferTransactions);

                if (match != null)
                {
                    transfers.Add(BankRecordsToTransfer(transfer, match));
                }
            }

            return transfers;
        }

        private static Transfer BankRecordsToTransfer(BankRecord from, BankRecord to)
        {
            return new Transfer
            {
                Id = Guid.NewGuid(),
                FromRecordId = from.Id,
                ToRecordId = to.Id,
                Amount = from.Amount,
                Date = from.Date,
                Description = from.Description + " / " + to.Description,
                IsUserCreated = false
            };
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

        private static BankRecord? FindTransferMatch(BankRecord transfer, List<BankRecord> candidates)
        {
            return candidates.FirstOrDefault(t =>
                t != transfer &&
                t.Amount == -transfer.Amount &&
                t.Date.Date == transfer.Date.Date);
        }
    }
}