namespace FinanceManager.Application.UseCases.Categories;

using FinanceManager.Application.Interfaces;
using FinanceManager.Application.UseCases;

public sealed record GetCategoryListResult(
    IReadOnlyList<CategoryDto> Categories,
    IReadOnlyDictionary<Guid, int> CategoryCountByGroupId
) : UseCaseResult;

public sealed class GetCategoryList(ICategoryRepository categoryRepository)
{
    public async Task<GetCategoryListResult> ExecuteAsync()
    {
        var categories = (await categoryRepository.GetAllAsync())
            .OrderBy(c => c.Group.Name)
            .ThenBy(c => c.Name)
            .Select(c => new CategoryDto(c.Id, c.Name, c.GroupId, c.Group.Name, c.Group.IsIncome))
            .ToList();

        var countByGroupId = categories
            .GroupBy(c => c.GroupId)
            .ToDictionary(g => g.Key, g => g.Count());

        return new GetCategoryListResult(categories, countByGroupId);
    }
}

public sealed record CategoryDto(Guid Id, string Name, Guid GroupId, string GroupName, bool IsIncome);
