namespace FinanceManager.Application.UseCases.Transactions;

using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Entities;

public sealed record GetCategoriesForTransactionsResult(List<Category> Categories) : UseCaseResult;

public sealed class GetCategoriesForTransactions(ICategoryRepository categoryRepository)
{
    public async Task<GetCategoriesForTransactionsResult> ExecuteAsync()
    {
        var categories = await categoryRepository.GetAllAsync();
        return new GetCategoriesForTransactionsResult([.. categories.OrderBy(c => c.Group.Name).ThenBy(c => c.Name)]);
    }
}
