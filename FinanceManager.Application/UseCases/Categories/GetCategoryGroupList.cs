namespace FinanceManager.Application.UseCases.Categories;

using FinanceManager.Application.Interfaces;
using FinanceManager.Application.UseCases;

public sealed record GetCategoryGroupsResult(IReadOnlyList<CategoryGroupDto> Groups) : UseCaseResult;

public sealed class GetCategoryGroupList(ICategoryRepository categoryRepository)
{
    public async Task<GetCategoryGroupsResult> ExecuteAsync()
    {
        var groups = (await categoryRepository.GetAllGroupsAsync())
            .OrderBy(g => g.IsIncome ? 0 : 1)
            .ThenBy(g => g.Name)
            .Select(g => new CategoryGroupDto(g.Id, g.Name, g.IsIncome))
            .ToList();

        return new GetCategoryGroupsResult(groups);
    }
}

public sealed record CategoryGroupDto(Guid Id, string Name, bool IsIncome);