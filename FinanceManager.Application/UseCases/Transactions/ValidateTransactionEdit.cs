using FinanceManager.Application.Constants;
using FinanceManager.Application.DTOs;
using FinanceManager.Application.Enums;
using FinanceManager.Application.Interfaces;

namespace FinanceManager.Application.UseCases.Transactions;

public sealed record ValidateTransactionEditResult(IEnumerable<UseCaseError> Errors) : UseCaseResult(Errors);

public sealed class ValidateTransactionEdit(ITransactionRepository transactionRepository, IBankRecordRepository bankRecordRepository)
{
    public Task<ValidateTransactionEditResult> ExecuteAsync(TransactionSummary summary)
    {
        var errors = new List<UseCaseValidationError>();
       
        var transaction = transactionRepository.GetByIdAsync(summary.TransactionId).Result;
        var bankRecord = bankRecordRepository.GetByIdAsync(transaction.RecordId).Result;

        if (summary.Category is null)
        {
            errors.Add(new UseCaseValidationError(summary.TransactionId, ValidationField.Category, ErrorMessages.CategoryRequired));
        }
        if (summary.Date > bankRecord.Date)
        {
            errors.Add(new UseCaseValidationError(summary.TransactionId, ValidationField.Date, ErrorMessages.DateMustNotBeAfterRecordDate));
        }
        if (summary.Amount <  Math.Min(0, bankRecord.Amount) || summary.Amount > Math.Max(0, bankRecord.Amount))
        {
            errors.Add(new UseCaseValidationError(summary.TransactionId, ValidationField.Amount, ErrorMessages.AmountMustBeBetweenZeroAndOriginalAmount));
        }
        if (summary.Amount == 0)
        {
            errors.Add(new UseCaseValidationError(summary.TransactionId, ValidationField.Amount, ErrorMessages.AmountMustNotBeZero));
        }

        return Task.FromResult(new ValidateTransactionEditResult(errors));
    }
}