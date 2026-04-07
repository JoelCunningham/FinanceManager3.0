namespace FinanceManager.Application.UseCases.Categories;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Interfaces;
using FinanceManager.Application.UseCases;

public sealed record GetCategoryListResult(
    IReadOnlyList<CategorySummary> Categories,
    IReadOnlyList<CategoryGroupSummary> Groups
) : UseCaseResult;

public sealed class GetCategoryList(ICategoryRepository categoryRepository)
{
    public async Task<GetCategoryListResult> ExecuteAsync()
    {
        var categories = (await categoryRepository.GetAllAsync())
            .OrderBy(c => c.Group.Name).ThenBy(c => c.Name)
            .Select(CategorySummary.FromCategory).ToList();

        var groups = (await categoryRepository.GetAllGroupsAsync())
            .OrderBy(g => g.IsIncome ? 0 : 1).ThenBy(g => g.Name)
            .Select(CategoryGroupSummary.FromCategoryGroup).ToList();

        return new GetCategoryListResult(categories, groups);
    }
}
