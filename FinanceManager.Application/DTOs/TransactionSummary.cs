namespace FinanceManager.Application.DTOs
{
    public class TransactionSummary
    {
        public string? CategoryName { get; set; }
        public required string Description { get; set; }
        public required decimal Amount { get; set; }
        public required DateTime Date { get; set; }
    }
}