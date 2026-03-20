namespace FinanceManager.WebApp.Components.Pages;

using FinanceManager.Application.Services;
using FinanceManager.Domain.Entities;
using FinanceManager.WebApp.Models;
using Havit.Blazor.Components.Web;
using Microsoft.AspNetCore.Components;

public partial class Categories : ComponentBase
{
    [Inject] public CategoryService CategoryService { get; set; } = default!;
    [Inject] public IHxMessengerService Messenger { get; set; } = default!;

    public ValidationModel Validation { get; set; } = new();

    public IReadOnlyList<Category> AllCategories { get; set; } = [];
    public IReadOnlyList<CategoryGroup> CategoryGroups { get; set; } = [];

    private Dictionary<Guid, int> CategoryCountByGroupId { get; set; } = [];

    private string GroupSearch { get; set; } = string.Empty;
    private string CategorySearch { get; set; } = string.Empty;

    private IEnumerable<CategoryGroup> FilteredGroups => CategoryGroups
        .Where(g => string.IsNullOrWhiteSpace(GroupSearch) || g.Name.Contains(GroupSearch, StringComparison.OrdinalIgnoreCase))
        .OrderByDescending(g => g.IsIncome).ThenBy(g => g.Name);

    private IEnumerable<Category> FilteredCategories => AllCategories
        .Where(c => string.IsNullOrWhiteSpace(CategorySearch) || c.Name.Contains(CategorySearch, StringComparison.OrdinalIgnoreCase) || c.Group.Name.Contains(CategorySearch, StringComparison.OrdinalIgnoreCase))
        .OrderByDescending(c => c.Group.IsIncome).ThenBy(c => c.Group.Name).ThenBy(c => c.Name);

    private int GetCategoryCount(Guid groupId) => CategoryCountByGroupId.TryGetValue(groupId, out var count) ? count : 0;

    protected override async Task OnInitializedAsync()
    {
        Validation.Messenger = Messenger;

        CategoryGroups = await CategoryService.GetCategoryGroupsAsync();
        AllCategories = await CategoryService.GetCategoriesAsync();

        CategoryCountByGroupId = AllCategories.GroupBy(c => c.GroupId).ToDictionary(g => g.Key, g => g.Count());
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
