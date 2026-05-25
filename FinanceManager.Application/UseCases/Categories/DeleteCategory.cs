namespace FinanceManager.Application.UseCases.Categories;

using FinanceManager.Application.Interfaces;
using FinanceManager.Application.UseCases;

public sealed record DeleteCategoryResult(IEnumerable<UseCaseError> Errors) : UseCaseResult(Errors);

public sealed class DeleteCategory(ICategoryRepository categoryRepository, ITransactionRepository transactionRepository, IDataStore dataStore)
{
    public async Task<DeleteCategoryResult> ExecuteAsync(Guid categoryId)
    {
        try
        {
            if (await transactionRepository.HasTransactionsForCategoryAsync(categoryId))
            {
                return new DeleteCategoryResult([new UseCaseInvalidOperationError("This category cannot be deleted because it contains transactions.")]);
            }

            await categoryRepository.DeleteAsync(categoryId);
            await dataStore.SaveAsync();

            return new DeleteCategoryResult([]);
        }
        catch
        {
            // TODO: Log exception
            return new DeleteCategoryResult([new UseCaseUnexpectedError()]);
        }
    }
}

