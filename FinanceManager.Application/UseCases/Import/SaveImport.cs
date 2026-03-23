namespace FinanceManager.Application.UseCases.Import;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Interfaces;
using FinanceManager.Application.UseCases;
using FinanceManager.Application.Utilities;

public sealed record ImportSaveResult(
    Guid ImportId = default,
    int RecordsSaved = default,
    int TransactionsSaved = default,
    int TransfersSaved = default,
    string? ErrorMessage = null
) : UseCaseResult(ErrorMessage);

public sealed class SaveImport(IBankRecordRepository bankRecordRepository, ITransactionRepository transactionRepository, ITransferRepository transferRepository, IUnitOfWork unitOfWork)
{
    public async Task<ImportSaveResult> ExecuteAsync(IEnumerable<ParsedTransaction> parsedTransactions)
    {
        var importId = Guid.NewGuid();
        await using var transaction = unitOfWork.BeginTransaction();

        try
        {
            var records = parsedTransactions.Select(t => t.ToBankRecord(importId)).ToList();

            await bankRecordRepository.CreateAsync(records);

            var transfers = EntityConverter.BankRecordsToTransfers(records.Where(t => t.IsInternalTransfer)).ToList();
            var transactions = records.Where(t => !t.IsInternalTransfer).Select(EntityConverter.BankRecordToTransaction).ToList();

            await transferRepository.CreateAsync(transfers);
            await transactionRepository.CreateAsync(transactions);

            await transaction.CommitAsync();

            return new ImportSaveResult(importId, transactions.Count, transfers.Count, records.Count);
        }
        catch
        {
            await transaction.RollbackAsync();
            return new ImportSaveResult(ErrorMessage: "An error occurred while saving the import.");
        }
    }
}
