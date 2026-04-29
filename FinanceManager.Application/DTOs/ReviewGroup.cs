namespace FinanceManager.Application.DTOs;

using FinanceManager.Application.DTOs.Base;
using FinanceManager.Domain.Entities;

public class ReviewGroup : ITransactionConvertible<ReviewGroup>
{
    public required Transaction InitialTransaction { get; set; }
    public List<ReviewTransaction> Transactions { get; set; } = [];
    public TransactionSummary? Transfers { get; set; }
    public bool IsIncome => InitialTransaction.Amount > 0;

    public static ReviewGroup FromTransaction(Transaction transaction)
    {
        return new ReviewGroup
        {
            InitialTransaction = transaction,
            Transactions = [ReviewTransaction.FromSummary(TransactionSummary.FromTransaction(transaction))],
        };
    }

    public void Reset()
    {
        Transfers = null;
        Transactions = [ReviewTransaction.FromSummary(TransactionSummary.FromTransaction(InitialTransaction))];
    }

    public void Split()
    {
        Transactions.Add(new()
        {
            TransactionId = Guid.NewGuid(),
            Record = BankRecordSummary.FromBankRecord(InitialTransaction.Record),
            Amount = 0,
            Date = InitialTransaction.Date,
            Description = InitialTransaction.Description,
            Category = null,
            Reimburses = null,
        });
    }

    public void Unsplit(ReviewTransaction transaction)
    {
        if (!Transactions.Contains(transaction))
        {
            throw new InvalidOperationException("Transaction does not belong to this InitialTransaction.");
        }
        if (Transactions.Count <= 1)
        {
            throw new InvalidOperationException("Only one transaction exists.");
        }
        Transactions.Remove(transaction);
        Transactions.First().Amount += transaction.Amount;
    }
}

public class ReviewTransaction : TransactionSummary
{
    public TransactionSummary? Reimburses { get; set; }
    public bool IsAutoCategorised { get; set; }

    public static ReviewTransaction FromSummary(TransactionSummary summary)
    {
        return new ReviewTransaction
        {
            TransactionId = summary.TransactionId,
            Amount = summary.Amount,
            Date = summary.Date,
            Description = summary.Description,
            Category = summary.Category,
            Record = summary.Record,
        };
    }

    public Transaction ToTransaction(IEnumerable<Transaction>? siblings = null)
    {
        return new Transaction
        {
            Id = TransactionId,
            Description = Description,
            Amount = Amount,
            Date = Date,
            RecordId = Record.BankRecordId,
            Record = Record.ToBankRecord(),
            CategoryId = Category?.Id,
            Category = Category?.ToCategory(),
            Siblings = siblings?.ToList(),
        };
    }
}