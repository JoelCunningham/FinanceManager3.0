using FinanceManager.Application.Interfaces;
using FinanceManager.Application.Utilities;
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
            var parsed = (await parser.ParseTransactionsFileAsync(file));

            var records = parsed.Select(t => t.ToBankRecord(bank, importId)).ToList();

            var duplicates = (await bankRecordRepository.FindDuplicatesAsync(records));

            return records.Except(duplicates);
        }

        public async Task<bool> SaveAsync(IEnumerable<BankRecord> importedTransactions)
        {
            var success = await bankRecordRepository.SaveAsync(importedTransactions);

            if (success)
            {
                var transfers = TypeConverter.BankRecordsToTransfers(importedTransactions);
                var transactions = importedTransactions.Select(TypeConverter.BankRecordToTransaction);

                success = await transferRepository.SaveAsync(transfers);
                success &= await transactionRepository.SaveAsync(transactions);
                //TODO add rollbacks
            }

            return success;
        }
    }
}