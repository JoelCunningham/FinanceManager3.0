using FinanceManager.Domain.Entities.Base;

namespace FinanceManager.Domain.Entities
{
    public class BankRecord : IEntity
    {
        public Guid Id { get; set; }
        public Guid ImportId { get; set; }

        public required string Bank { get; set; }
        public string? AccountNumber { get; set; }
        public bool IsInternalTransfer { get; set; }

        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public required string Description { get; set; }
        public string? Type { get; set; }
        public string? Reference { get; set; }

        public ICollection<Transaction> Transactions { get; set; } = [];
    }
}
