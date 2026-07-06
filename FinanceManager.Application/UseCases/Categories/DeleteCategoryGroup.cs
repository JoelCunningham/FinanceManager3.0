namespace FinanceManager.Application.UseCases.Categories;

using FinanceManager.Application.Interfaces;
using FinanceManager.Application.UseCases;

public sealed record DeleteCategoryGroupResult(IEnumerable<UseCaseError> Errors) : UseCaseResult(Errors);

public sealed class DeleteCategoryGroup(ICategoryGroupRepository categoryGroupRepository, ICategoryRepository categoryRepository, ITransactionRepository transactionRepository, IDataStore dataStore)
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
            // TODO: Log exception
            return new DeleteCategoryGroupResult([new UseCaseUnexpectedError()]);
        }
    }
}

