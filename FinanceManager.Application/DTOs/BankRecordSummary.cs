namespace FinanceManager.Application.DTOs;

using FinanceManager.Domain.Entities;

public class BankRecordSummary
{
    public Guid BankRecordId { get; set; }

    public required string Bank { get; set; }
    public string? AccountNumber { get; set; }

    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public required string Description { get; set; }
    public string? Type { get; set; }
    public string? Reference { get; set; }

    public static BankRecordSummary FromBankRecord(BankRecord record)
    {
        return new BankRecordSummary
        {
            BankRecordId = record.Id,
            Bank = record.Bank,
            AccountNumber = record.AccountNumber,
            Amount = record.Amount,
            Date = record.Date,
            Description = record.Description,
            Type = record.Type,
            Reference = record.Reference,
        };
    }
}
