namespace FinanceManager.Application.UseCases;

using FinanceManager.Application.UseCases.Categories;

public sealed class CategoriesWorkflow(GetCategoryList getCategoryList, GetCategoryGroupList getCategoryGroupList)
{
    public Task<GetCategoryListResult> GetCategoriesAsync() => getCategoryList.ExecuteAsync();
    public Task<GetCategoryGroupsResult> GetCategoryGroupsAsync() => getCategoryGroupList.ExecuteAsync();
}
