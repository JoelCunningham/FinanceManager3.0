namespace FinanceManager.Application.DTOs;

using FinanceManager.Application.DTOs.Base;
using FinanceManager.Domain.Entities;

public class TransactionSummary : ITransactionConvertible<TransactionSummary>
{
    public Guid EntityId { get; set; }
    public CategorySummary? Category { get; set; }
    public required string Description { get; set; }
    public required decimal Amount { get; set; }
    public required DateTime Date { get; set; }
    public required BankRecordSummary Record { get; set; }

    public static TransactionSummary FromTransaction(Transaction transaction)
    {
        return new TransactionSummary
        {
            EntityId = transaction.Id,
            Amount = transaction.TotalAmount,
            Date = transaction.Date,
            Description = transaction.Description,
            Category = transaction.Category is not null ? CategorySummary.FromCategory(transaction.Category) : null,
            Record = BankRecordSummary.FromBankRecord(transaction.Record),
        };
    }

    public Transaction ToTransaction(IEnumerable<Transaction>? siblings = null)
    {
        return new Transaction
        {
            Id = EntityId,
            Description = Description,
            Amount = Amount,
            Date = Date,
            RecordId = Record.BankRecordId,
            Record = Record.ToBankRecord(),
            CategoryId = Category?.Id,
            Category = Category?.ToCategory(),
            Siblings = siblings?.ToList() ?? [],
        };
    }

    public TransactionSummary Clone()
    {
        return new TransactionSummary
        {
            EntityId = EntityId,
            Amount = Amount,
            Date = Date,
            Description = Description,
            Category = Category,
            Record = Record,
        };
    }
}
