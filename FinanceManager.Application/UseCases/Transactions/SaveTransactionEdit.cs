namespace FinanceManager.Application.UseCases.Transactions;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Interfaces;
using Microsoft.Extensions.Logging;

public sealed record SaveTransactionEditResult(IEnumerable<UseCaseError> Errors) : UseCaseResult(Errors);

public sealed class SaveTransactionEdit(ITransactionRepository transactionRepository, IMachineLearningRepository machineLearningRepository, IDataStore dataStore, ILogger<SaveTransactionEdit> logger)
{
    public async Task<SaveTransactionEditResult> ExecuteAsync(TransactionSummary transaction)
    {
        if (transaction.Category is null)
        {
            return new SaveTransactionEditResult([new UseCaseInvalidOperationError("Transaction must have a category.")]);
        }

        try
        {
            var entity = await transactionRepository.GetByIdAsync(transaction.EntityId);

            if (transaction.Category.Id != entity.CategoryId && entity.CategoryId is not null)
            {
                await machineLearningRepository.CreateAsync(entity.CategoryId.Value, entity.Record.Description);
            }

            entity.Date = transaction.Date;
            entity.Description = transaction.Description;
            entity.Amount = transaction.Amount;
            entity.CategoryId = transaction.Category.Id;
            
            await transactionRepository.UpdateAsync(entity);     

            await dataStore.SaveAsync();
            return new SaveTransactionEditResult([]);
        }
        catch
        {
            logger.LogError("An unexpected error occurred while saving transaction with ID {TransactionId}.", transaction.EntityId);
            return new SaveTransactionEditResult([new UseCaseUnexpectedError()]);
        }
    }
}