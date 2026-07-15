namespace FinanceManager.Domain.Entities;

using FinanceManager.Domain.Entities.Base;

public class BankRecord : UserOwnedEntity
{
    public Guid ImportId { get; set; }

    public Guid BankAccountId { get; set; }
    public required BankAccount BankAccount { get; set; }

    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public required string Description { get; set; }
    public string? Type { get; set; }
    public string? Reference { get; set; }

    public bool IsInternalTransfer { get; set; }

    public ICollection<Transaction> Transactions { get; set; } = [];
}