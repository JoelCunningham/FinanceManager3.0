namespace FinanceManager.Application.DTOs; 

using FinanceManager.Domain.Entities;

public class ParsedTransaction
{
    public Guid Id { get; set; }
    public required string Bank { get; set; }
    public string? AccountNumber { get; set; }
    public required DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public required string Description { get; set; }
    public required string Type { get; set; }
    public string? Reference { get; set; }
    public bool IsInternalTransfer { get; set; }

    public BankRecord ToBankRecord(Guid importId, BankAccount bankAccount)
    {
        return new BankRecord
        {
            Id = Id,
            ImportId = importId,
            BankAccountId = bankAccount.Id,
            BankAccount = bankAccount,
            Amount = Amount,
            Date = Date,
            Description = Description,
            Type = Type,
            Reference = Reference,
            IsInternalTransfer = IsInternalTransfer,
        };
    }
}
