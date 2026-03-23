namespace FinanceManager.Application.UseCases.Review;

using FinanceManager.Application.DTOs;

public sealed record ValidateReviewGroupResult(
    bool IsValid, 
    IReadOnlyList<ReviewValidationError> Errors
) : UseCaseResult;

public sealed class ValidateReviewGroup
{
    public static Task<ValidateReviewGroupResult> ExecuteAsync(ReviewGroup group)
    {
        var errors = new List<ReviewValidationError>();

        foreach (var transaction in group.Transactions)
        {
            if (transaction.Category is null && group.Transfers is null && transaction.Reimburses is null)
            {
                errors.Add(new ReviewValidationError(transaction.Id, "category", "A category is required"));
            }

            if (transaction.Amount == 0)
            {
                errors.Add(new ReviewValidationError(transaction.Id, "amount", "Amount must not be zero"));
            }
        }

        var isValid = errors.Count == 0;
        return Task.FromResult(new ValidateReviewGroupResult(isValid, errors));
    }
}

public sealed record ReviewValidationError(Guid TransactionId, string Key, string Message);
