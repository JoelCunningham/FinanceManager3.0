namespace FinanceManager.Application.DTOs;

public class TransactionDetails
{
    public required TransactionSummary TransactionSummary { get; set; } 
    public required BankRecordSummary RecordSummary { get; set; }
    public IEnumerable<TransactionSummary> Siblings { get; set; } = [];
    public IEnumerable<ReimbursementSummary> Reimbursements { get; set; } = [];
}