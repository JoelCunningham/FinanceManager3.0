using FinanceManager.Domain.Entities;

namespace FinanceManager.Application.DTOs
{
    public class ImportedTransaction
    {
        public required BankRecord BankRecord { get; set; }
        public required decimal Amount { get; set; }
        public required DateTime Date { get; set; }
        public required string Description { get; set; }
        public string? Category { get; set; }

        public ImportedTransaction? Transfers { get; set; }
        public ImportedTransaction? Reimburses { get; set; }
        public ICollection<ImportedTransaction> Splits { get; set; } = [];

        public void Backdate(DateTime date)
        {
            if (date > BankRecord.Date)
            {
                throw new InvalidOperationException("Backdate must be before or equal to the bank record date");
            }
            Date = date;
        }

        public ImportedTransaction Split()
        {
            var newSplit = new ImportedTransaction
            {
                BankRecord = BankRecord,
                Amount = 0,
                Date = Date,
                Description = Description,
                Category = Category
            };

            foreach (var split in Splits)
            {
                newSplit.Splits.Add(split);
            }
            Splits.Add(newSplit);

            return newSplit;
        }

        public void SetAmount(decimal amount)
        {
            if (amount < 0 || amount > BankRecord.Amount)
                throw new InvalidOperationException("Amount must be between 0 and the bank record amount.");

            if (Splits.Count == 0)
                throw new InvalidOperationException("No splits to set amount for.");

            var diff = Amount - amount;

            if (diff > 0)
            {
                Splits.First().Amount += diff;
            }
            else
            {
                foreach (var split in Splits)
                {
                    if (diff == 0)
                    {
                        break;
                    }
                    if (diff > 0)
                    {
                        var toRemove = Math.Min(split.Amount, -diff);
                        split.Amount -= toRemove;
                        diff += toRemove;
                    }
                }
            }

            Amount = amount;
        }


        public void Unsplit()
        {
            if (Splits.Count == 0)
            {
                throw new InvalidOperationException("No splits to unsplit.");
            }

            Splits.First().Amount += Amount;

            foreach (var split in Splits)
            {
                split.Splits.Remove(this);
            }
        }
    }

}
