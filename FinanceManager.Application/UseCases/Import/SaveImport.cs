namespace FinanceManager.Application.UseCases.Import;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Interfaces;
using FinanceManager.Application.UseCases;
using FinanceManager.Application.Utilities;

public sealed class ImportSaveResult : UseCaseResult
{
    public Guid ImportId { get; init; }
    public int RecordsSaved { get; init; }
    public int TransactionsSaved { get; init; }
    public int TransfersSaved { get; init; }
}

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

            return new ImportSaveResult
            {
                ImportId = importId,
                TransactionsSaved = transactions.Count,
                TransfersSaved = transfers.Count,
                RecordsSaved = records.Count,
            };
        }
        catch
        {
            await transaction.RollbackAsync();
            return new ImportSaveResult { ErrorMessage = "An error occurred while saving the import." };
        }
    }
}
