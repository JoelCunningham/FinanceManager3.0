namespace FinanceManager.Application.UseCases.Review;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Interfaces;

public sealed record GetReviewGroupResult(ReviewGroup Group) : UseCaseResult();

public sealed class GetReviewGroup(ITransactionRepository transactionRepository)
{
    public async Task<GetReviewGroupResult> ExecuteAsync(Guid transactionId)
    {
        var transaction = await transactionRepository.GetByIdAsync(transactionId);
        var recordTransactions = await transactionRepository.GetByRecordIdAsync(transaction.RecordId);
        var reimbursements = await transactionRepository.GetReimbursementsAsync(recordTransactions.Select(t => t.Id));
        var reviewGroup = ReviewGroup.FromTransactions(transaction.Record, recordTransactions, reimbursements);

        return new GetReviewGroupResult(reviewGroup);
    }
}
