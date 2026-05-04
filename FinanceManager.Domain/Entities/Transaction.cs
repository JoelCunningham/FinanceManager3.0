namespace FinanceManager.Domain.Entities;

using FinanceManager.Domain.Entities.Base;

public sealed class Transaction : IEntity
{
    public Guid Id { get; set; }

    public Guid RecordId { get; set; }
    public required BankRecord Record { get; set; }

    public Guid? CategoryId { get; set; }
    public Category? Category { get; set; }

    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public required string Description { get; set; }

    public Guid? ReimbursesId { get; set; }
    public Transaction? Reimburses { get; set; }
    public ICollection<Transaction> Reimbursements { get; set; } = [];

    public bool IsReimbursement => ReimbursesId.HasValue;
    public decimal TotalAmount => Amount + (Reimbursements?.Sum(r => r.Amount) ?? 0);
}