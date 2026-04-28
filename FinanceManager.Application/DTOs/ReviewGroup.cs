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
            Transactions = [ReviewTransaction.FromTransaction(transaction)],
        };
    }

    public void Reset()
    {
        Transfers = null;
        Transactions = [ReviewTransaction.FromTransaction(InitialTransaction)];
    }

    public void Split()
    {
        Transactions.Add(new()
        {
            Id = Guid.NewGuid(),
            Record = InitialTransaction.Record,
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

public class ReviewTransaction : ITransactionConvertible<ReviewTransaction>
{
    public Guid Id { get; set; }
    public required BankRecord Record { get; set; }
    public required string Description { get; set; }
    public required decimal Amount { get; set; }
    public required DateTime Date { get; set; }
    public CategorySummary? Category { get; set; }
    public TransactionSummary? Reimburses { get; set; }
    public bool IsAutoCategorised { get; set; }

    public static ReviewTransaction FromTransaction(Transaction transaction)
    {
        var category = transaction.Category is not null ? CategorySummary.FromCategory(transaction.Category) : null;

        return new ReviewTransaction
        {
            Id = transaction.Id,
            Record = transaction.Record,
            Description = transaction.Description,
            Amount = transaction.Amount,
            Date = transaction.Date,
            Category = category,
            Reimburses = null,
        };
    }

    public Transaction ToTransaction(IEnumerable<Transaction>? siblings = null)
    {
        return new Transaction
        {
            Id = Id,
            RecordId = Record.Id,
            Record = Record,
            Description = Description,
            Amount = Amount,
            Date = Date,
            CategoryId = Category?.Id,
            Category = Category?.ToCategory(),
            Siblings = siblings?.ToList(),
        };
    }
}