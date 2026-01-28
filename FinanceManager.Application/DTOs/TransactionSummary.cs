using FinanceManager.Domain.Entities;

namespace FinanceManager.Application.DTOs
{
    public class TransactionSummary
    {
        public string? CategoryName { get; set; }
        public required string Description { get; set; }
        public required decimal Amount { get; set; }
        public required DateTime Date { get; set; }

        public static TransactionSummary FromTransaction(Transaction transaction)
        {
            return new TransactionSummary
            {
                CategoryName = transaction.CategoryId.ToString(),
                Description = transaction.Description,
                Amount = transaction.Amount,
                Date = transaction.Date
            };
        }
    }
}