namespace FinanceManager.Application.UseCases.Review;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Enums;

public sealed record BackdateTransactionResult(IEnumerable<UseCaseError> Errors) : UseCaseResult(Errors);

public sealed class BackdateTransaction()
{
    public async Task<BackdateTransactionResult> ExecuteAsync(ReviewTransaction transaction, DateTime date, DateTime initialDate)
    {
        if (date > initialDate)
        {
            return new BackdateTransactionResult([new UseCaseValidationError(transaction.Id, ValidationField.Date, "Date must be before the original date")]);
        }
        transaction.Date = date;
        return new BackdateTransactionResult([]);
    }
}
