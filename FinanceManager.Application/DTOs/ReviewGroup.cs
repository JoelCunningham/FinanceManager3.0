namespace FinanceManager.Application.DTOs;

using FinanceManager.Domain.Entities;

public class ReviewGroup
{
    public required BankRecordSummary Record { get; set; }
    public List<ReviewTransaction> InitialTransactions { get; set; } = [];
    public List<ReviewTransaction> Transactions { get; set; } = [];
    public TransactionSummary? Transfers { get; set; }
    public bool IsIncome => Record.Amount > 0;

    public static ReviewGroup FromTransactions(BankRecord record, IEnumerable<Transaction> transactions, IEnumerable<Transaction> reimbursements)
    {
        var reimbursementsMap = reimbursements
            .Where(t => t.ReimbursesId is not null)
            .GroupBy(t => t.ReimbursesId!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        var reviewTransactions = transactions.Select(t =>
        {
            var reviewTransaction = ReviewTransaction.FromTransaction(t);
            if (reimbursementsMap.TryGetValue(t.Id, out var reimbursements))
            {
                reviewTransaction.Reimbursements = [.. reimbursements.Select(ReviewTransaction.FromTransaction)];
            }

            return reviewTransaction;
        }).ToList();

        return new ReviewGroup
        {
            Record = BankRecordSummary.FromBankRecord(record),
            Transactions = reviewTransactions,
            InitialTransactions = [.. reviewTransactions],
        };
    }

    public void Reset()
    {
        Transfers = null;
        Transactions = InitialTransactions;
    }

    public void Split(ReviewTransaction transaction)
    {
        if (!Transactions.Contains(transaction))
        {
            throw new InvalidOperationException("Transaction does not belong to this ReviewGroup.");
        }
        Transactions.Add(new()
        {
            EntityId = Guid.NewGuid(),
            Record = Record,
            Amount = 0,
            Date = transaction.Date,
            Description = transaction.Description,
            Category = null,
            Reimburses = null,
        });
    }

    public void Unsplit(ReviewTransaction transaction)
    {
        if (!Transactions.Contains(transaction))
        {
            throw new InvalidOperationException("Transaction does not belong to this ReviewGroup.");
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
    public TransactionSummary? Reimburses { get; set; } // Used for adding a reimbursement to an existing transaction
    public IEnumerable<ReviewTransaction> Reimbursements { get; set; } = []; // Used for editing reimbursements of an existing transaction
    public bool IsAutoCategorised { get; set; }

    public static new ReviewTransaction FromTransaction(Transaction transaction)
    {
        var record = BankRecordSummary.FromBankRecord(transaction.Record);
        var category = transaction.Category != null ? CategorySummary.FromCategory(transaction.Category) : null;
        var reimbursements = transaction.Reimbursements.Select(FromTransaction).ToList();

        return new ReviewTransaction
        {
            EntityId = transaction.Id,
            Amount = transaction.Amount,
            Date = transaction.Date,
            Description = transaction.Description,
            Category = category,
            Record = record,
            Reimbursements = reimbursements,
        };
    }

    public Transaction ToTransaction(BankRecordSummary record)
    {
        return new Transaction
        {
            Id = EntityId,
            Description = Description,
            Amount = Amount,
            Date = Date,
            RecordId = record.BankRecordId,
            Record = record.ToBankRecord(),
            CategoryId = Category?.Id,
            Category = null,
        };
    }
}