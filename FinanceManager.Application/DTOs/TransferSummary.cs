using FinanceManager.Domain.Entities;

namespace FinanceManager.Application.DTOs
{
    public class TransferSummary
    {
        public Guid EntityId { get; set; }
        public decimal Amount { get; set; }
        public required Transferable From { get; set; }
        public required Transferable To { get; set; }
        public DateTime Date { get; set; }
        public required string Description { get; set; }
        public bool IsUserCreated { get; set; }

        public static TransferSummary FromTransfer(Transfer transfer, BankRecord fromRecord, BankRecord toRecord)
        {
            return new TransferSummary
            {
                EntityId = transfer.Id,
                Amount = transfer.Amount,
                Date = transfer.Date,
                Description = transfer.Description,
                IsUserCreated = transfer.IsUserCreated,
                From = new Transferable
                {
                    Bank = fromRecord.Bank,
                    Account = fromRecord.AccountNumber,
                },
                To = new Transferable
                {
                    Bank = toRecord.Bank,
                    Account = toRecord.AccountNumber,
                }
            };
        }
    }

    public class Transferable
    {
        public required string Bank { get; set; }
        public string? Account { get; set; }
    }
}
