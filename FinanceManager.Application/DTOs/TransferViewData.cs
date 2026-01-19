namespace FinanceManager.Application.DTOs
{
    public class TransferViewData
    {
        public decimal Amount { get; set; }
        public required Transferable From { get; set; }
        public required Transferable To { get; set; }
        public DateTime Date { get; set; }
        public required string Description { get; set; }
        public bool IsUserCreated { get; set; }
    }

    public class Transferable
    {
        public required string Bank { get; set; }
        public string? Account { get; set; }
    }
}
