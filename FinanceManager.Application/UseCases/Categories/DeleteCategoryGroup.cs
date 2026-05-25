namespace FinanceManager.Application.UseCases.Categories;

using FinanceManager.Application.Interfaces;
using FinanceManager.Application.UseCases;

public sealed record DeleteCategoryGroupResult(IEnumerable<UseCaseError> Errors) : UseCaseResult(Errors);

public sealed class DeleteCategoryGroup(ICategoryGroupRepository categoryGroupRepository, ITransactionRepository transactionRepository, IDataStore dataStore)
{
    public async Task<DeleteCategoryGroupResult> ExecuteAsync(Guid groupId)
    {
        try
        {
            if (await transactionRepository.HasTransactionsForCategoryGroupAsync(groupId))
            {
                return new DeleteCategoryGroupResult([new UseCaseInvalidOperationError("This area cannot be deleted because it contains transactions.")]);
            }

            await categoryGroupRepository.DeleteAsync(groupId);
            await dataStore.SaveAsync();

            return new DeleteCategoryGroupResult([]);
        }
        catch
        {
            // TODO: Log exception
            return new DeleteCategoryGroupResult([new UseCaseUnexpectedError()]);
        }
    }
}

