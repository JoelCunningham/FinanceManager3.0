namespace FinanceManager.Domain.Entities;

using FinanceManager.Domain.Entities.Base;

public sealed class Reimbursement : IEntity
{
    public Guid Id { get; set; }

    public Guid TransactionId { get; set; }
    public required Transaction Transaction { get; set; }

    public required Guid RecordId { get; set; }
    public required BankRecord Record { get; set; }

    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public required string Description { get; set; }
    
    public ICollection<Transaction>? Siblings { get; set; }
}