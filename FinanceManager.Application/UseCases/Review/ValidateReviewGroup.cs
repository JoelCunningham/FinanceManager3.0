namespace FinanceManager.Application.UseCases.Review;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Utilities;

public sealed record ValidateReviewGroupResult(IEnumerable<UseCaseError> Errors) : UseCaseResult(Errors);

public sealed class ValidateReviewGroup
{
    public Task<ValidateReviewGroupResult> ExecuteAsync(ReviewGroup group)
    {
        var errors = new List<UseCaseValidationError>();

        foreach (var transaction in group.Transactions)
        {
            TransactionHelper.ValidateTransaction(transaction, group.InitialTransaction.Date, group.InitialTransaction.Amount).ToList().ForEach(errors.Add);
        }

        return Task.FromResult(new ValidateReviewGroupResult(errors));
    }
}
