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
            TransactionSummary = TransactionSummary.FromTransactions(transaction),
            RecordSummary = BankRecordSummary.FromBankRecord(transaction.Record),
            Reimbursements = reimbursements.Select(TransactionSummary.FromTransactions),
            Siblings = siblings.Select(TransactionSummary.FromTransactions),
        };

        return new GetTransactionDetailsResult(transactionDetails);
    }
}