namespace FinanceManager.Application.UseCases.Review;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.UseCases;
using FinanceManager.Application.Utilities;

public sealed record ValidateReviewGroupResult(IEnumerable<UseCaseError> Errors) : UseCaseResult(Errors);

public sealed class ValidateReviewGroup
{
    public static Task<ValidateReviewGroupResult> ExecuteAsync(ReviewGroup group)
    {
        var errors = new List<UseCaseValidationError>();

        foreach (var transaction in group.Transactions)
        {
            var originalDate = group.Record.Date;
            var originalAmount = group.Record.Amount;
            var requireCategory = transaction.Reimburses is null && group.Transfers is null;
            TransactionHelper.ValidateTransaction(transaction, originalDate, originalAmount, requireCategory).ToList().ForEach(errors.Add);
        }

        return Task.FromResult(new ValidateReviewGroupResult(errors));
    }
}
