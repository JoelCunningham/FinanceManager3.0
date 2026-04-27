namespace FinanceManager.Application.DTOs;

using FinanceManager.Domain.Entities;

public class ReimbursementSummary
{
    public Guid ReimbursementId { get; set; }
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public required string Description { get; set; }

    public static ReimbursementSummary FromReimbursement(Reimbursement reimbursement)
    {
        return new ReimbursementSummary
        {
            ReimbursementId = reimbursement.Id,
            Date = reimbursement.Date,
            Amount = reimbursement.Amount,
            Description = reimbursement.Description,
        };
    }
}
