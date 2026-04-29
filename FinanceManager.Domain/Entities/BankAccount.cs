namespace FinanceManager.Domain.Entities;

using FinanceManager.Domain.Entities.Base;

public class BankAccount : IEntity
{
    public Guid Id { get; set; }
    public required string Bank { get; set; }
    public string? AccountNumber { get; set; } 
    public string? Name { get; set; }

    public string DisplayName => Name ?? $"{Bank} {AccountNumber}";

    public ICollection<BankRecord> BankRecords { get; set; } = [];
}
