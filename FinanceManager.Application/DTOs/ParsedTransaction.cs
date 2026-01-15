using FinanceManager.Domain.Entities;
using System.Transactions;

namespace FinanceManager.Application.DTOs
{
    public class ParsedTransaction
    {
        public string? AccountNumber { get; set; }
        public required DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public required string Description { get; set; }
        public required string Type { get; set; }
        public string? Reference { get; set; }
        public bool IsInternalTransfer { get; set; }

        public BankRecord ToBankRecord(string bank, Guid importId)
        {
            return new BankRecord
            {
                Id = Guid.NewGuid(),
                ImportId = importId,
                Bank = bank,
                AccountNumber = AccountNumber,
                Amount = Amount,
                Date = Date,
                Description = Description,
                Type = Type,
                Reference = Reference,
                IsInternalTransfer = IsInternalTransfer,
            };
        }
    }
}
