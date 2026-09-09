namespace FinanceManager.Application.UseCases.Import;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Interfaces;
using FinanceManager.Application.UseCases;
using FinanceManager.Application.Utilities;
using Microsoft.Extensions.Logging;

public sealed record SaveImportResult(
    Guid ImportId,
    int RecordsSaved,
    int TransactionsSaved,
    int TransfersSaved,
    IEnumerable<UseCaseError> Errors
) : UseCaseResult(Errors);

public sealed class SaveImport(IBankRecordRepository bankRecordRepository, IBankAccountRepository bankAccountRepository, ITransactionRepository transactionRepository, ITransferRepository transferRepository, ILogger<SaveImport> logger)
{
    public async Task<SaveImportResult> ExecuteAsync(IEnumerable<ParsedTransaction> parsedTransactions)
    {
        var importId = Guid.NewGuid();

        try
        {
            var accounts = await bankAccountRepository.GetOrCreateAsync(parsedTransactions.First().Bank, parsedTransactions.Select(t => t.AccountNumber).Distinct());
            var records = parsedTransactions.Select(t => t.ToBankRecord(importId, accounts.First(a => a.Bank == t.Bank && a.AccountNumber == t.AccountNumber))).ToList();

            await bankRecordRepository.CreateAsync(records);

            var transfers = EntityConverter.BankRecordsToTransfers(records.Where(t => t.IsInternalTransfer)).ToList();
            var transactions = EntityConverter.BankRecordsToTransactions(records.Where(t => !t.IsInternalTransfer)).ToList();

            await transferRepository.CreateAsync(transfers);
            await transactionRepository.CreateAsync(transactions);
        
            return new SaveImportResult(importId, transactions.Count, transfers.Count, records.Count, []);
        }
        catch
        {
            logger.LogError("An unexpected error occurred while saving the import.");
            return new SaveImportResult(Guid.Empty, 0, 0, 0, [new UseCaseUnexpectedError()]);
        }
    }
}
