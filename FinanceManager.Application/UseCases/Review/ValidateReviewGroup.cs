namespace FinanceManager.Application.UseCases.Review;

using FinanceManager.Application.Constants;
using FinanceManager.Application.DTOs;
using FinanceManager.Application.Enums;

// TODO Remove this and replace with common ValidateTransaction
public sealed record ValidateReviewGroupResult(IEnumerable<UseCaseError> Errors) : UseCaseResult(Errors);

public sealed class ValidateReviewGroup
{
    public static Task<ValidateReviewGroupResult> ExecuteAsync(ReviewGroup group)
    {
        var errors = new List<UseCaseValidationError>();

        foreach (var transaction in group.Transactions)
        {
            if (transaction.Category is null && group.Transfers is null && transaction.Reimburses is null)
            {
                errors.Add(new UseCaseValidationError(transaction.Id, ValidationField.Category, ErrorMessages.CategoryRequired));
            }

            if (transaction.Amount == 0)
            {
                errors.Add(new UseCaseValidationError(transaction.Id, ValidationField.Amount, ErrorMessages.AmountMustNotBeZero));
            }
        }

        return Task.FromResult(new ValidateReviewGroupResult(errors));
    }
}
