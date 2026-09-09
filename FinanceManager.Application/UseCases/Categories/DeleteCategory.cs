namespace FinanceManager.Application.UseCases.Categories;

using FinanceManager.Application.Interfaces;
using FinanceManager.Application.UseCases;
using Microsoft.Extensions.Logging;

public sealed record DeleteCategoryResult(IEnumerable<UseCaseError> Errors) : UseCaseResult(Errors);

public sealed class DeleteCategory(ICategoryRepository categoryRepository, ITransactionRepository transactionRepository, ILogger<DeleteCategory> logger)
{
    public async Task<DeleteCategoryResult> ExecuteAsync(Guid categoryId)
    {
        try
        {
            var transactions = await transactionRepository.GetByCategoryIdAsync(categoryId);
            foreach (var transaction in transactions)
            {
                transaction.CategoryId = null;
            }

            await categoryRepository.DeleteAsync(categoryId);
            return new DeleteCategoryResult([]);
        }
        catch
        {
            logger.LogError("An unexpected error occurred while deleting category with ID {CategoryId}.", categoryId);
            return new DeleteCategoryResult([new UseCaseUnexpectedError()]);
        }
    }
}

