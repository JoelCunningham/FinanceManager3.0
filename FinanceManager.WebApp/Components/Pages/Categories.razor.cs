namespace FinanceManager.WebApp.Components.Pages;

using FinanceManager.Application.UseCases.Categories;
using FinanceManager.WebApp.Models;
using Havit.Blazor.Components.Web;
using Microsoft.AspNetCore.Components;

public partial class Categories : ComponentBase
{
    [Inject] public GetCategoryList GetCategoryList { get; set; } = default!;
    [Inject] public GetCategoryGroupList GetCategoryGroupList { get; set; } = default!;
    [Inject] public IHxMessengerService Messenger { get; set; } = default!;

    public ValidationModel Validation { get; set; } = new();

    public IReadOnlyList<CategoryDto> AllCategories { get; set; } = [];
    public IReadOnlyList<CategoryGroupDto> CategoryGroups { get; set; } = [];

    private IReadOnlyDictionary<Guid, int> CategoryCountByGroupId { get; set; } = new Dictionary<Guid, int>();

    private string GroupSearch { get; set; } = string.Empty;
    private string CategorySearch { get; set; } = string.Empty;

    private IEnumerable<CategoryGroupDto> FilteredGroups => CategoryGroups
        .Where(g => string.IsNullOrWhiteSpace(GroupSearch) || g.Name.Contains(GroupSearch, StringComparison.OrdinalIgnoreCase))
        .OrderByDescending(g => g.IsIncome).ThenBy(g => g.Name);

    private IEnumerable<CategoryDto> FilteredCategories => AllCategories
        .Where(c => string.IsNullOrWhiteSpace(CategorySearch) || c.Name.Contains(CategorySearch, StringComparison.OrdinalIgnoreCase) || c.GroupName.Contains(CategorySearch, StringComparison.OrdinalIgnoreCase))
        .OrderByDescending(c => c.IsIncome).ThenBy(c => c.GroupName).ThenBy(c => c.Name);

    private int GetCategoryCount(Guid groupId) => CategoryCountByGroupId.TryGetValue(groupId, out var count) ? count : 0;

    protected override async Task OnInitializedAsync()
    {
        Validation.Messenger = Messenger;

        var categoriesResult = await GetCategoryList.ExecuteAsync();
        CategoryGroups = (await GetCategoryGroupList.ExecuteAsync()).Groups;
        AllCategories = categoriesResult.Categories;
        CategoryCountByGroupId = categoriesResult.CategoryCountByGroupId;
    }

    private void OnGroupSearchChanged(string value)
    {
        GroupSearch = value ?? string.Empty;
    }

    private void OnCategorySearchChanged(string value)
    {
        CategorySearch = value ?? string.Empty;
    }
}
