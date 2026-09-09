namespace FinanceManager.WebApp.Components.Pages.Features.Categories;

using FinanceManager.Application.Constants.Navigation;
using FinanceManager.Application.DTOs;
using FinanceManager.Application.Enums;
using FinanceManager.Application.UseCases;
using FinanceManager.WebApp.Components.Base;
using FinanceManager.WebApp.Components.Shared.Modals;
using Microsoft.AspNetCore.Components;

[Route(Pages.Categories)]
public partial class _Page : MainPageBase
{
    [Inject] protected Application.UseCases.Categories.GetCategories GetCategoriesUseCase { get; set; } = default!;
    [Inject] protected Application.UseCases.Categories.SaveCategoryGroupEdit SaveCategoryGroupEditUseCase { get; set; } = default!;

    public IReadOnlyList<CategorySummary> AllCategories { get; set; } = [];
    public IReadOnlyList<CategoryGroupSummary> CategoryGroups { get; set; } = [];

    public string GroupSearch { get; set; } = string.Empty;
    public CategoryGroupMode GroupMode { get; set; } = CategoryGroupMode.All;

    public CategoryGroupSummary? CreateGroup { get; set; } = null;
    public CategoryGroupModal GroupModal { get; set; } = new();

    public IEnumerable<CategoryGroupSummary> FilteredGroups => CategoryGroups
        .Where(g => string.IsNullOrWhiteSpace(GroupSearch) || g.Name.Contains(GroupSearch, StringComparison.OrdinalIgnoreCase))
        .Where(g => GroupMode == CategoryGroupMode.All || (GroupMode == CategoryGroupMode.Expense && !g.IsIncome) || (GroupMode == CategoryGroupMode.Income && g.IsIncome))
        .OrderByDescending(g => g.IsIncome).ThenBy(g => g.Name);

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        await RefreshCategories();
    }

    private async Task RefreshCategories()
    {
        var allCategories = await GetCategoriesUseCase.ExecuteAsync();
        CategoryGroups = allCategories.Groups;
        AllCategories = allCategories.Categories;
    }

    private void OnGroupSearchChanged(string value)
    {
        GroupSearch = value ?? string.Empty;
    }

    private void OnGroupModeChanged(CategoryGroupMode mode)
    {
        GroupMode = mode;
    }

    private async Task OnGroupCreate()
    {
        Validation.Clear();
        if (CreateGroup is null) return;

        var result = await SaveCategoryGroupEditUseCase.ExecuteAsync(CreateGroup);
        if (!result.IsSuccess)
        {
            Validation.SetErrors(result.Errors);
        }
        else
        {
            await RefreshCategories();
            await CloseGroupCreateModal();
            Validation.SetSuccess("Area created successfully.");
        }
    }

    private async Task OpenGroupCreateModal()
    {
        CreateGroup = CategoryGroupSummary.Empty(Guid.NewGuid());
        await GroupModal.ShowAsync();
    }

    private async Task CloseGroupCreateModal()
    {
        await GroupModal.HideAsync();
    }

    private void OpenGroupPanel(CategoryGroupSummary group)
    {
        Navigation.NavigateTo($"{Pages.Categories}/{group.Name}");
    }
}
