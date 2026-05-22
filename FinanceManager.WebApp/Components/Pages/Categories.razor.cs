namespace FinanceManager.WebApp.Components.Pages;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Enums;
using FinanceManager.Application.UseCases;
using FinanceManager.WebApp.Components.Base;
using FinanceManager.WebApp.Components.Features.Categories;

public partial class Categories : PageBase
{
    public IReadOnlyList<CategorySummary> AllCategories { get; set; } = [];
    public IReadOnlyList<CategoryGroupSummary> CategoryGroups { get; set; } = [];

    public string GroupSearch { get; set; } = string.Empty;
    public CategoryGroupMode GroupMode { get; set; } = CategoryGroupMode.All;

    public bool IsEditing { get; set; } = false;
    public CategoryGroupSummary? CurrentGroup { get; set; } = null;
    public IEnumerable<CategorySummary> SelectedGroupCategories => AllCategories.Where(c => c.GroupId == CurrentGroup?.Id);

    public CategoryGroupModal GroupModal { get; set; } = new();

    public IEnumerable<CategoryGroupSummary> FilteredGroups => CategoryGroups
        .Where(g => string.IsNullOrWhiteSpace(GroupSearch) || g.Name.Contains(GroupSearch, StringComparison.OrdinalIgnoreCase))
        .Where(g => GroupMode == CategoryGroupMode.All || (GroupMode == CategoryGroupMode.Expense && !g.IsIncome) || (GroupMode == CategoryGroupMode.Income && g.IsIncome))
        .OrderByDescending(g => g.IsIncome).ThenBy(g => g.Name);

    protected override async Task OnInitializedAsync()
    {
        Validation.Messenger = Messenger;

        var categoriesResult = await UseCases.GetCategoryListAsync();
        CategoryGroups = categoriesResult.Groups;
        AllCategories = categoriesResult.Categories;
    }

    private void OnGroupSearchChanged(string value)
    {
        GroupSearch = value ?? string.Empty;
    }

    private void OnGroupModeChanged(CategoryGroupMode mode)
    {
        GroupMode = mode;
    }

    private async Task OnGroupSave()
    {
        if (CurrentGroup is null)
        {
            return;
        }

        await UseCases.SaveCategoryGroupEditAsync(CurrentGroup);

        await CloseGroupModal();
    }

    private async Task OpenGroupEditModal()
    {
        IsEditing = true;
        await GroupModal.ShowAsync();
    }

    private async Task OpenGroupCreateModal()
    {
        CurrentGroup = new CategoryGroupSummary { Id = Guid.NewGuid(), Name = string.Empty, Colour = string.Empty, IsIncome = false };
        IsEditing = false;
        await GroupModal.ShowAsync();
    }

    private async Task CloseGroupModal()
    {
        await GroupModal.HideAsync();
    }

    private void OpenGroupPanel(CategoryGroupSummary group)
    {
        CurrentGroup = group;
    }

    private void CloseGroupPanel()
    {
        CurrentGroup = null;
    }
}
