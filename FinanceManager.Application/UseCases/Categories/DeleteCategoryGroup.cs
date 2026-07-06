namespace FinanceManager.Application.UseCases.Categories;

using FinanceManager.Application.Interfaces;
using FinanceManager.Application.UseCases;
using Microsoft.Extensions.Logging;

public sealed record DeleteCategoryGroupResult(IEnumerable<UseCaseError> Errors) : UseCaseResult(Errors);

public sealed class DeleteCategoryGroup(ICategoryGroupRepository categoryGroupRepository, ICategoryRepository categoryRepository, ITransactionRepository transactionRepository, IDataStore dataStore, ILogger<DeleteCategoryGroup> logger)
{
    public async Task<DeleteCategoryGroupResult> ExecuteAsync(Guid groupId)
    {
        try
        {
            var categories = await categoryRepository.GetByGroupIdAsync(groupId);
            foreach (var category in categories)
            {
                var transactions = await transactionRepository.GetByCategoryIdAsync(category.Id);
                foreach (var transaction in transactions)
                {
                    transaction.CategoryId = null;
                }

                await categoryRepository.DeleteAsync(category.Id);
            }

            await categoryGroupRepository.DeleteAsync(groupId);
            await dataStore.SaveAsync();

            return new DeleteCategoryGroupResult([]);
        }
        catch
        {
            logger.LogError("An unexpected error occurred while deleting category group with ID {GroupId}.", groupId);
            return new DeleteCategoryGroupResult([new UseCaseUnexpectedError()]);
        }
    }
}

