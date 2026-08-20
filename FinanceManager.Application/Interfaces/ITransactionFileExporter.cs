using FinanceManager.Domain.Entities;

namespace FinanceManager.Application.Interfaces;

public interface ITransactionFileExporter
{
    byte[] ExportBankRecords(IEnumerable<BankRecord> bankRecords);
    byte[] ExportTransactions(IEnumerable<Transaction> transactions);
    byte[] ExportTransfers(IEnumerable<Transfer> transfers);
}
