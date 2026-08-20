namespace FinanceManager.Infrastructure.Exporters;

using CsvHelper;
using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Entities;
using System.Globalization;

public class TransactionFileExporter : ITransactionFileExporter
{
    public byte[] ExportBankRecords(IEnumerable<BankRecord> bankRecords)
    {
        var exportBankRecords = bankRecords.Select(record => new ExportBankRecord(
            record.BankAccount?.AccountNumber,
            record.Date,
            record.Amount,
            record.Description,
            record.Type,
            record.Reference,
            record.BankAccount?.Bank
        ));
        return ExportData(exportBankRecords);
    }

    public byte[] ExportTransactions(IEnumerable<Transaction> transactions)
    {
        var exportTransactions = transactions.Select(transaction => new ExportTransaction(
            transaction.Record.BankAccount?.AccountNumber,
            transaction.Date,
            transaction.Amount,
            transaction.Description,
            transaction.Category?.Group.Name,
            transaction.Category?.Name,
            transaction.Record.BankAccount?.Bank
        ));
        return ExportData(exportTransactions);
    }

    public byte[] ExportTransfers(IEnumerable<Transfer> transfers)
    {
        var exportTransfers = transfers.Select(transfer => new ExportTransfer(
            transfer.FromRecord.BankAccount?.AccountNumber,
            transfer.ToRecord.BankAccount?.AccountNumber,
            transfer.Date,
            transfer.Amount,
            transfer.Description,
            transfer.FromRecord.BankAccount?.Bank
        ));
        return ExportData(exportTransfers);
    }

    private static byte[] ExportData<T>(IEnumerable<T> data)
    {
        using var stream = new MemoryStream();
        using var writer = new StreamWriter(stream);
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

        csv.WriteRecords(data);
        writer.Flush();

        return stream.ToArray();
    }

    private record ExportBankRecord(
        string? AccountNumber,
        DateTime Date,
        decimal Amount,
        string Description,
        string? Type,
        string? Reference,
        string? Bank
    );

    private record ExportTransaction(
        string? AccountNumber,
        DateTime Date,
        decimal Amount,
        string Description,
        string? CategoryGroup,
        string? Category,
        string? Bank
    );

    private record ExportTransfer(
        string? FromAccountNumber,
        string? ToAccountNumber,
        DateTime Date,
        decimal Amount,
        string Description,
        string? Bank
    );
}