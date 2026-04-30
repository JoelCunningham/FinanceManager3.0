namespace FinanceManager.Application.UseCases.Transactions;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Interfaces;

public sealed record SaveTransactionEditResult(IEnumerable<UseCaseError> Errors) : UseCaseResult(Errors);

public sealed class SaveTransactionEdit(ITransactionRepository transactionRepository, IMachineLearningRepository machineLearningRepository, IUnitOfWork unitOfWork)
{
    public async Task<SaveTransactionEditResult> ExecuteAsync(TransactionSummary transaction)
    {
        await using var operations = unitOfWork.BeginTransaction();

        try
        {
            var entity = await transactionRepository.GetByIdAsync(transaction.EntityId);

            entity.Date = transaction.Date;
            entity.Description = transaction.Description;
            entity.Amount = transaction.Amount;
            entity.Category = transaction.Category!.ToCategory();
            entity.CategoryId = transaction.Category.Id;
            
            await transactionRepository.UpdateAsync(entity);
            await machineLearningRepository.SaveAsync(entity.Category, entity.Description);

            await operations.CommitAsync();
            return new SaveTransactionEditResult([]);
        }
        catch
        {
            // TODO: Log exception
            await operations.RollbackAsync();
            return new SaveTransactionEditResult([new UseCaseUnexpectedError()]);
        }
    }
}