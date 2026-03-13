namespace FinanceManager.Domain.Entities;

using FinanceManager.Domain.Entities.Base;

public sealed class Transfer : IEntity
{
    public Guid Id { get; set; }

    public required Guid FromRecordId { get; set; }
    public required BankRecord FromRecord { get; set; }
    public required Guid ToRecordId { get; set; }
    public required BankRecord ToRecord { get; set; }

    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public required string Description { get; set; }
    public bool IsUserCreated { get; set; }
}