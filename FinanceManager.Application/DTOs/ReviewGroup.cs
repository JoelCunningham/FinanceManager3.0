namespace FinanceManager.Application.DTOs;

using FinanceManager.Application.DTOs.Base;
using FinanceManager.Domain.Entities;

public class ReviewGroup : ITransactionConvertible<ReviewGroup>
{
    public required Transaction InitalTransaction { get; set; }
    public List<ReviewTransaction> Transactions { get; set; } = [];
    public TransactionSummary? Transfers { get; set; }
    public bool IsIncome => InitalTransaction.Amount > 0;

    public static ReviewGroup FromTransaction(Transaction transaction)
    {
        return new ReviewGroup
        {
            InitalTransaction = transaction,
            Transactions = [ReviewTransaction.FromTransaction(transaction)],
        };
    }

    public void Reset()
    {
        Transfers = null;
        Transactions = [ReviewTransaction.FromTransaction(InitalTransaction)];
    }

    public void Backdate(ReviewTransaction transaction, DateTime date)
    {
        if (!Transactions.Contains(transaction))
        {
            throw new InvalidOperationException("Transaction does not belong to this InitalTransaction.");
        }
        if (date > InitalTransaction.Date)
        {
            throw new InvalidOperationException("Backdate must be before or equal to the InitalTransaction date");
        }
        transaction.Date = date;
    }

    public void Split()
    {
        Transactions.Add(new()
        {
            Id = Guid.NewGuid(),
            Record = InitalTransaction.Record,
            Amount = 0,
            Date = InitalTransaction.Date,
            Description = InitalTransaction.Description,
            Category = null,
            Reimburses = null,
        });
    }

    public void Unsplit(ReviewTransaction transaction)
    {
        if (!Transactions.Contains(transaction))
        {
            throw new InvalidOperationException("Transaction does not belong to this InitalTransaction.");
        }
        if (Transactions.Count <= 1)
        {
            throw new InvalidOperationException("Only one transaction exists.");
        }
        Transactions.Remove(transaction);
        Transactions.First().Amount += transaction.Amount;
    }

    public void SetAmount(ReviewTransaction transaction, decimal amount)
    {
        if (!Transactions.Contains(transaction))
        {
            throw new InvalidOperationException("Transaction does not belong to this InitialTransaction.");
        }
        if (amount < Math.Min(0, InitalTransaction.Amount) || amount > Math.Max(0, InitalTransaction.Amount))
        {
            throw new InvalidOperationException("Amount must be between 0 and the InitialTransaction amount.");
        }

        var diff = Math.Abs(amount) - Math.Abs(transaction.Amount);
        var current = Transactions.IndexOf(transaction);
        var nextTransactions = Transactions.Skip(current + 1).Concat(Transactions.Take(current));

        foreach (var split in nextTransactions)
        {
            if (diff == 0) break;

            var absSplit = Math.Abs(split.Amount);

            if (diff > 0)
            {
                var reducible = absSplit;
                var reduction = Math.Min(diff, reducible);

                absSplit -= reduction;
                diff -= reduction;
            }
            else
            {
                var currentAllocated = Transactions.Sum(t => Math.Abs(t.Amount));
                var remainingCapacity = Math.Abs(InitalTransaction.Amount) - currentAllocated;

                var increase = Math.Min(-diff, remainingCapacity);

                absSplit += increase;
                diff += increase;
            }

            split.Amount = absSplit * Math.Sign(InitalTransaction.Amount);
        }

        transaction.Amount = amount;
    }
}

public class ReviewTransaction : ITransactionConvertible<ReviewTransaction>
{
    public Guid Id { get; set; }
    public required BankRecord Record { get; set; }
    public required string Description { get; set; }
    public required decimal Amount { get; set; }
    public required DateTime Date { get; set; }
    public Category? Category { get; set; }
    public TransactionSummary? Reimburses { get; set; }
    public bool IsAutoCategorised { get; set; }

    public static ReviewTransaction FromTransaction(Transaction transaction)
    {
        return new ReviewTransaction
        {
            Id = transaction.Id,
            Record = transaction.Record,
            Description = transaction.Description,
            Amount = transaction.Amount,
            Date = transaction.Date,
            Category = transaction.Category,
            Reimburses = null,
        };
    }

    public Transaction ToTransaction()
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
            Category = Category,
        };
    }
}