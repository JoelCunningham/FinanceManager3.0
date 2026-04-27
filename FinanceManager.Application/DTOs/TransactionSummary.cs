namespace FinanceManager.Application.DTOs;

using FinanceManager.Application.DTOs.Base;
using FinanceManager.Domain.Entities;

public class TransactionSummary : ITransactionConvertible<TransactionSummary>
{
    public Guid TransactionId { get; set; }
    public CategorySummary? Category { get; set; }
    public required string Description { get; set; }
    public required decimal Amount { get; set; }
    public required DateTime Date { get; set; }
    public required BankRecordSummary Record { get; set; }

    public static TransactionSummary FromTransaction(Transaction transaction)
    {
        return new TransactionSummary
        {
            TransactionId = transaction.Id,
            Amount = transaction.TotalAmount,
            Date = transaction.Date,
            Description = transaction.Description,
            Category = transaction.Category is not null ? CategorySummary.FromCategory(transaction.Category) : null,
            Record = BankRecordSummary.FromBankRecord(transaction.Record),
        };
    }

    public TransactionSummary Clone()
    {
        return new TransactionSummary
        {
            TransactionId = TransactionId,
            Amount = Amount,
            Date = Date,
            Description = Description,
            Category = Category, 
            Record = Record,
        };
    }
}
