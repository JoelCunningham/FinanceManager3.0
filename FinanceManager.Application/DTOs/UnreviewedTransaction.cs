using FinanceManager.Domain.Entities;

namespace FinanceManager.Application.DTOs
{
    public class UnreviewedTransaction
    {
        public required BankRecord Record { get; set; }
        public required IEnumerable<InnerTransaction> InnerTransactions { get; set; }

        public static UnreviewedTransaction FromBankRecord(BankRecord record)
        {
            return new UnreviewedTransaction
            {
                Record = record,
                InnerTransactions = [new()
                {
                    Description = record.Description,
                    Amount = record.Amount,
                    Date = record.Date,
                }]
            };
        }

        public void Reset()
        {
            InnerTransactions = [new() {
                Amount = Record.Amount,
                Description = Record.Description,
                Date = Record.Date,
            }];
        }

        public void Backdate(InnerTransaction transaction, DateTime date)
        {
            IsTransactionInRecord(transaction);

            if (date > Record.Date)
            {
                throw new InvalidOperationException("Backdate must be before or equal to the record date");
            }

            transaction.Date = date;
        }

        public void ResetDate(InnerTransaction transaction)
        {
            IsTransactionInRecord(transaction);
            transaction.Date = Record.Date;
        }

        public void Split()
        {
            var split = new InnerTransaction
            {
                Amount = 0,
                Date = Record.Date,
                Description = Record.Description,
                Category = null,
            };
            InnerTransactions = InnerTransactions.Append(split);
        }

        public void Unsplit(InnerTransaction transaction)
        {
            IsTransactionInRecord(transaction);

            if (InnerTransactions.Count() <= 1)
            {
                throw new InvalidOperationException("No splits to unsplit.");
            }
            InnerTransactions = InnerTransactions.Where(t => t != transaction);

            // TODO Set amount of remaining transactions proportionally
        }

        public void SetAmount(InnerTransaction transaction, decimal amount)
        {
            IsTransactionInRecord(transaction);

            if (amount < 0 || amount > Record.Amount)
                throw new InvalidOperationException("Amount must be between 0 and the record amount.");

            if (amount == transaction.Amount)
                return;

            var diff = transaction.Amount - amount;
            var current = InnerTransactions.ToList().IndexOf(transaction);
            var nextTransactions = InnerTransactions.Skip(current + 1).Concat(InnerTransactions.Take(current));

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

        private void IsTransactionInRecord(InnerTransaction transaction)
        {
            if (!InnerTransactions.Contains(transaction))
                throw new InvalidOperationException("Transaction does not belong to this record.");
        }
    }

    public class InnerTransaction
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
