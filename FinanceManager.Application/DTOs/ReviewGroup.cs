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
        List<ReviewTransaction> transactions = [ReviewTransaction.FromTransaction(transaction)];
        if (transaction.Siblings != null && transaction.Siblings.Count != 0)
        {
            transactions.AddRange(transaction.Siblings.Select(ReviewTransaction.FromTransaction));
        }

        return new ReviewGroup
        {
            InitialTransaction = transaction,
            Transactions = transactions,
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
            EntityId = Guid.NewGuid(),
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
    public TransactionSummary? Reimburses { get; set; } // Used for adding a reimbursement to an existing transaction
    public IEnumerable<ReviewTransaction>? Reimbursements { get; set; } // Used for editing reimbursements of an existing transaction
    public bool IsAutoCategorised { get; set; }

    public static new ReviewTransaction FromTransaction(Transaction transaction)
    {
        var record = BankRecordSummary.FromBankRecord(transaction.Record);
        var category = transaction.Category != null ? CategorySummary.FromCategory(transaction.Category) : null;
        var reimbursements = transaction.Reimbursements?.Select(FromReimbursement).ToList();

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

    public new Transaction ToTransaction(IEnumerable<Transaction>? siblings = null)
    {
        //TODO investigate if siblings parameter is necessary for reimbursements
        var reimbursements = Reimbursements?.Select(r => r.ToReimbursement([], true)).ToList();

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
            Siblings = siblings?.ToList(),
            Reimbursements = reimbursements,
        };
    }

    public static ReviewTransaction FromReimbursement(Reimbursement reimbursement)
    {
        return new ReviewTransaction
        {
            EntityId = reimbursement.Id,
            Amount = reimbursement.Amount,
            Date = reimbursement.Date,
            Description = reimbursement.Description,
            Category = null,
            Record = BankRecordSummary.FromBankRecord(reimbursement.Record),
        };
    }

    public Reimbursement ToReimbursement(IEnumerable<Transaction>? siblings = null, bool reimbursesThis = false)
    {
        if (Reimburses is null && !reimbursesThis) throw new InvalidOperationException("Reimburses property must exist to convert to reimbursement.");

        var reimbursesEntity = Reimburses is not null ? Reimburses.ToTransaction(siblings) : ToTransaction(siblings);

        return new Reimbursement
        {
            Id = EntityId,
            TransactionId = reimbursesEntity.Id,
            Transaction = reimbursesEntity,
            Amount = Amount,
            Date = Date,
            Description = Description,
            RecordId = Record.BankRecordId,
            Record = Record.ToBankRecord(),
            Siblings = siblings?.ToList(),
        };
    }
}