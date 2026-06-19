namespace FinanceManager.Application.UseCases.Categories;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Interfaces;
using FinanceManager.Application.UseCases;

public sealed record GetCategoriesResult(
    IReadOnlyList<CategorySummary> Categories,
    IReadOnlyList<CategoryGroupSummary> Groups
) : UseCaseResult;

public sealed class GetCategories(ICategoryRepository categoryRepository, ICategoryGroupRepository categoryGroupRepository)
{
    public async Task<GetCategoriesResult> ExecuteAsync()
    {
        var categories = (await categoryRepository.GetAllAsync())
            .OrderBy(c => c.Group.Name).ThenBy(c => c.Name)
            .Select(CategorySummary.FromCategory).ToList();

        var groups = (await categoryGroupRepository.GetAllAsync())
            .OrderBy(g => g.IsIncome ? 0 : 1).ThenBy(g => g.Name)
            .Select(CategoryGroupSummary.FromCategoryGroup).ToList();

        return new GetCategoriesResult(categories, groups);
    }
}
