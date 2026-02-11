using FinanceManager.Domain.Entities;

namespace FinanceManager.Application.DTOs
{
    public class TransactionSummary
    {
        public Guid Id { get; set; }
        public Category? Category { get; set; }
        public required string Description { get; set; }
        public required decimal Amount { get; set; }
        public required DateTime Date { get; set; }

        public static TransactionSummary FromTransaction(Transaction transaction)
        {
            var amount = transaction.Amount - (transaction.Reimbursements?.Sum(r => r.Amount) ?? 0);

            return new TransactionSummary
            {
                Id = transaction.Id,
                Category = transaction.Category,
                Description = transaction.Description,
                Amount = amount,
                Date = transaction.Date
            };
        }
    }
}