namespace FinanceManager.Application.UseCases.Transactions;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Interfaces;
using FinanceManager.Application.UseCases;

public sealed record GetCategoryGroupsResult(IReadOnlyList<CategoryGroupSummary> Groups) : UseCaseResult;

public sealed class GetCategoryGroupList(ICategoryRepository categoryRepository)
{
    public async Task<GetCategoryGroupsResult> ExecuteAsync()
    {
        var groups = (await categoryRepository.GetAllGroupsAsync())
            .OrderBy(g => g.IsIncome ? 0 : 1).ThenBy(g => g.Name)
            .Select(CategoryGroupSummary.FromCategoryGroup).ToList();

        return new GetCategoryGroupsResult(groups);
    }
}