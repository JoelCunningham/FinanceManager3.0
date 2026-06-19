namespace FinanceManager.Application.UseCases.Transactions;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Interfaces;

public sealed record GetTransactionDetailsResult(TransactionDetails Transaction) : UseCaseResult;

public sealed class GetTransactionDetails(ITransactionRepository transactionRepository)
{
    public async Task<GetTransactionDetailsResult> ExecuteAsync(Guid transactionId)
    {
        var transaction = await transactionRepository.GetByIdAsync(transactionId);
        var reimbursements = await transactionRepository.GetReimbursementsAsync([transactionId]);
        var siblings = (await transactionRepository.GetByRecordIdAsync(transaction.RecordId)).Where(t => t.Id != transactionId);

        var transactionDetails = new TransactionDetails
        {
            TransactionSummary = TransactionSummary.FromTransaction(transaction),
            RecordSummary = BankRecordSummary.FromBankRecord(transaction.Record),
            Reimbursements = reimbursements.Select(TransactionSummary.FromTransaction),
            Siblings = siblings.Select(TransactionSummary.FromTransaction),
        };

        return new GetTransactionDetailsResult(transactionDetails);
    }
}