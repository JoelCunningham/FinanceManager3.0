namespace FinanceManager.Application.UseCases.Review;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Interfaces;

public sealed record GetReviewGroupResult(ReviewGroup Group) : UseCaseResult();

public sealed class GetReviewGroup(ITransactionRepository transactionRepository)
{
    public async Task<GetReviewGroupResult> ExecuteAsync(Guid transactionId)
    {
        var transaction = await transactionRepository.GetByIdAsync(transactionId);

        return new GetReviewGroupResult
        (
            ReviewGroup.FromTransaction(transaction)
        );
    }
}
