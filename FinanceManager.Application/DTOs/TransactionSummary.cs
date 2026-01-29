using FinanceManager.Domain.Entities;

namespace FinanceManager.Application.DTOs
{
    public class TransactionSummary
    {
        public Category? Category { get; set; }
        public required string Description { get; set; }
        public required decimal Amount { get; set; }
        public required DateTime Date { get; set; }

        public static TransactionSummary FromTransaction(Transaction transaction)
        {
            var category = transaction.CategoryId is null ? null : new Category() { Id = transaction.CategoryId.Value, Name = transaction.CategoryId.Value.ToString() };

            return new TransactionSummary
            {
                Category = category,
                Description = transaction.Description,
                Amount = transaction.Amount,
                Date = transaction.Date
            };
        }
    }
}