namespace FinanceManager.Application.UseCases.Transactions;

using FinanceManager.Application.Enums;
using FinanceManager.Application.Interfaces;
using FinanceManager.Application.UseCases;

public sealed record ExportTransactionsResult(
    byte[] Content,
    string FileName,
    string ContentType,
    IEnumerable<UseCaseError> Errors
) : UseCaseResult(Errors);

public sealed class ExportTransactions(IBankRecordRepository bankRecordRepository, ITransactionRepository transactionRepository, ITransferRepository transferRepository, ITransactionFileExporter exporter)
{
    public async Task<ExportTransactionsResult> ExecuteAsync(ExportType exportType)
    {
        if (exportType == ExportType.BankRecords)
        {
            var bankRecords = await bankRecordRepository.GetAllAsync();
            var csvContent = exporter.ExportBankRecords(bankRecords);
            return new ExportTransactionsResult(csvContent, CreateFileName("BankRecords"), "text/csv", []);
        }
        else if (exportType == ExportType.Transactions)
        {
            var transactions = await transactionRepository.GetAllAsync();
            var csvContent = exporter.ExportTransactions(transactions);
            return new ExportTransactionsResult(csvContent, CreateFileName("Transactions"), "text/csv", []);
        }
        else if (exportType == ExportType.Transfers)
        {
            var transfers = await transferRepository.GetAllAsync();
            var csvContent = exporter.ExportTransfers(transfers);
            return new ExportTransactionsResult(csvContent, CreateFileName("Transfers"), "text/csv", []);
        }
        else
        {
            return new ExportTransactionsResult([], string.Empty, string.Empty, [new UseCaseInvalidOperationError("Invalid export type.")]);
        }
    }

    private static string CreateFileName(string baseName)
    {
        return DateTime.Now.ToString("yyyyMMdd") + "-FinanceManager-" + baseName + ".csv";
    }
}
