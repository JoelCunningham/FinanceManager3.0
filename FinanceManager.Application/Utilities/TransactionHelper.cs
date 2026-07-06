namespace FinanceManager.Application.Utilities;

using FinanceManager.Application.Constants;
using FinanceManager.Application.DTOs;
using FinanceManager.Application.Enums;
using FinanceManager.Application.UseCases;

public class TransactionHelper()
{
    public static IEnumerable<UseCaseValidationError> ValidateTransaction(TransactionSummary summary, DateTime recordDate, decimal recordAmount, bool requireCategory = true)
    {
        var errors = new List<UseCaseValidationError>();
        
        if (requireCategory && summary.Category is null)
        {
            errors.Add(new UseCaseValidationError(summary.EntityId, ValidationField.Category, ErrorMessages.CategoryRequired));
        }
        if (summary.Date > recordDate)
        {
            errors.Add(new UseCaseValidationError(summary.EntityId, ValidationField.Date, ErrorMessages.DateMustNotBeAfterRecordDate));
        }
        if (summary.Amount <  Math.Min(0, recordAmount) || summary.Amount > Math.Max(0, recordAmount))
        {
            errors.Add(new UseCaseValidationError(summary.EntityId, ValidationField.Amount, ErrorMessages.AmountMustBeBetweenZeroAndOriginalAmount));
        }
        if (summary.Amount == 0)
        {
            errors.Add(new UseCaseValidationError(summary.EntityId, ValidationField.Amount, ErrorMessages.AmountMustNotBeZero));
        }

        return errors;
    }
}
