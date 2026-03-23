namespace FinanceManager.Application.UseCases;

using FinanceManager.Application.UseCases.Categories;

public sealed class CategoriesWorkflow(GetCategoryList getCategoryList)
{
    public Task<GetCategoryListResult> GetCategoriesAsync() => getCategoryList.ExecuteAsync();
}
