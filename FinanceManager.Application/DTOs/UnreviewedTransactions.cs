using FinanceManager.Domain.Entities;

namespace FinanceManager.Application.DTOs
{
    public class UnreviewedTransactions
    {
        public required BankRecord Record { get; set; }
        public required IEnumerable<UncategorisedTransaction> Transactions { get; set; }

        public void Reset()
        {
            Transactions = [new() {
                Amount = Record.Amount,
                Description = Record.Description,
                Date = Record.Date,
            }];
        }

        public void Backdate(UncategorisedTransaction transaction, DateTime date)
        {
            IsTransactionInRecord(transaction);

            if (date > Record.Date)
            {
                throw new InvalidOperationException("Backdate must be before or equal to the record date");
            }

            transaction.Date = date;
        }

        public void ResetDate(UncategorisedTransaction transaction)
        {
            IsTransactionInRecord(transaction);
            transaction.Date = Record.Date;
        }

        public void Split()
        {
            var split = new UncategorisedTransaction
            {
                Amount = 0,
                Date = Record.Date,
                Description = Record.Description,
                Category = null,
            };
            Transactions = Transactions.Append(split);
        }

        public void Unsplit(UncategorisedTransaction transaction)
        {
            IsTransactionInRecord(transaction);

            if (Transactions.Count() <= 1)
            {
                throw new InvalidOperationException("No splits to unsplit.");
            }
            Transactions = Transactions.Where(t => t != transaction);

            // TODO Set amount of remaining transactions proportionally
        }

        public void SetAmount(UncategorisedTransaction transaction, decimal amount)
        {
            IsTransactionInRecord(transaction);

            if (amount < 0 || amount > Record.Amount)
                throw new InvalidOperationException("Amount must be between 0 and the record amount.");

            if (amount == transaction.Amount)
                return;

            var diff = transaction.Amount - amount;
            var current = Transactions.ToList().IndexOf(transaction);
            var nextTransactions = Transactions.Skip(current + 1).Concat(Transactions.Take(current));

            foreach (var split in nextTransactions)
            {
                if (diff == 0)
                {
                    break;
                }
                if (diff > 0) // Need to decrease transaction amount
                {
                    var toAdd = Math.Min(diff, Record.Amount - split.Amount);
                    split.Amount += toAdd;
                    diff -= toAdd;
                }
                if (diff < 0) // Need to increase transaction amount
                {
                    var toRemove = Math.Min(-diff, split.Amount);
                    split.Amount -= toRemove;
                    diff += toRemove;
                }

                transaction.Amount = amount;
            }
        }

        private void IsTransactionInRecord(UncategorisedTransaction transaction)
        {
            if (!Transactions.Contains(transaction))
                throw new InvalidOperationException("Transaction does not belong to this record.");
        }
    }

    public class UncategorisedTransaction
    {
        public required string Description { get; set; }
        public required decimal Amount { get; set; }
        public required DateTime Date { get; set; }

        public Category? Category { get; set; }
        public Relation Relation { get; set; } = Relation.None;
        public TransactionSummary? RelatedTransction { get; set; }
    }

    public enum Relation
    {
        None,
        Reimbursement,
        Transfer,
    }
}
