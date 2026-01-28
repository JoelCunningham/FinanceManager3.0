using FinanceManager.Application.DTOs;
using FinanceManager.Application.Interfaces;
using FinanceManager.Application.Utilities;

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
        public async Task<IEnumerable<ParsedTransaction>> ImportAsync(Stream file, string bank, string extension)
        {
            var parser = parserService.GetParser(bank, extension);
            var parsed = await parser.ParseTransactionsFileAsync(file);

            var records = parsed.Select(t => t.ToBankRecord(new Guid()));
            var duplicates = await bankRecordRepository.FindDuplicatesAsync(records);
            var duplicateIds = duplicates.Select(d => d.Id).ToHashSet();

            return parsed.Where(p => !duplicateIds.Contains(p.Id));
        }

        public async Task SaveAsync(IEnumerable<ParsedTransaction> parsedTransactions)
        {
            var importId = Guid.NewGuid();
            await using var transaction = _unitOfWork.BeginTransaction();

            try
            {
                var records = parsedTransactions.Select(t => t.ToBankRecord(importId)).ToList();

                await bankRecordRepository.SaveAsync(records);

                var transfers = EntityConverter.BankRecordsToTransfers(records.Where(t => t.IsInternalTransfer));
                var transactions = records.Where(t => !t.IsInternalTransfer).Select(EntityConverter.BankRecordToTransaction);

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