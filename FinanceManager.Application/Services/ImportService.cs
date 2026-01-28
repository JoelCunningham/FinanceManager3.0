using FinanceManager.Application.Interfaces;
using FinanceManager.Application.Utilities;
using FinanceManager.Domain.Entities;

namespace FinanceManager.Application.Services
{
    public class ImportService
    (
        ParserService parserService,
        IBankRecordRepository bankRecordRepository,
        ITransactionRepository transactionRepository,
        ITransferRepository transferRepository,
        IUnitOfWork _unitOfWork
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

        public async Task SaveAsync(IEnumerable<BankRecord> importedTransactions)
        {
            await using var transaction = _unitOfWork.BeginTransaction();

            try
            {
                await bankRecordRepository.SaveAsync(importedTransactions);

                var transfers = TypeConverter.BankRecordsToTransfers(importedTransactions.Where(t => t.IsInternalTransfer));
                var transactions = importedTransactions.Where(t => !t.IsInternalTransfer).Select(TypeConverter.BankRecordToTransaction);

                await transferRepository.SaveAsync(transfers);
                await transactionRepository.SaveAsync(transactions);

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}