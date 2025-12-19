namespace FinanceManager.Domain.Entities
{
    public class BankRecord
    {
        public int Id { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public decimal? CreditAmount { get; set; }
        public decimal? DebitAmount { get; set; }
        public DateTime Date { get; set; }
        public string Narrative { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;

        public decimal TotalAmount => (CreditAmount ?? 0) + (DebitAmount ?? 0) * -1;
    }
}
