namespace FinanceManager.Application.UseCases.Transactions;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Interfaces;
using FinanceManager.Application.Utilities;

public sealed record ValidateTransactionEditResult(IEnumerable<UseCaseError> Errors) : UseCaseResult(Errors);

public sealed class ValidateTransactionEdit(ITransactionRepository transactionRepository, IBankRecordRepository bankRecordRepository)
{
    public Task<ValidateTransactionEditResult> ExecuteAsync(TransactionSummary summary)
    {      
        var transaction = transactionRepository.GetByIdAsync(summary.TransactionId).Result;
        var bankRecord = bankRecordRepository.GetByIdAsync(transaction.RecordId).Result;

        var errors = TransactionHelper.ValidateTransaction(summary, bankRecord.Date, bankRecord.Amount);
        
        return Task.FromResult(new ValidateTransactionEditResult(errors));
    }
}