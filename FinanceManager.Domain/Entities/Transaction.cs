using FinanceManager.Domain.Entities.Base;

namespace FinanceManager.Domain.Entities
{
    public sealed class Transaction : IEntity
    {
        public Guid Id { get; set; }
        public Guid RecordId { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public required string Description { get; set; }
        public Guid? CategoryId { get; set; }
    }
}
