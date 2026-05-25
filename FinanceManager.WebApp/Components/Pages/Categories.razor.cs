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
    public CategoryGroupSummary? EditGroup { get; set; } = null;
    public CategorySummary? EditCategory { get; set; } = null;
    public IEnumerable<CategorySummary> CurrentGroupCategories => AllCategories.Where(c => c.GroupId == CurrentGroup?.Id);

    public CategoryModal CategoryModal { get; set; } = new();
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

    private async Task OnCategorySave()
    {
        Validation.Clear();
        if (EditCategory is null) return;

        var result = await UseCases.SaveCategoryEditAsync(EditCategory);
        if (!result.IsSuccess)
        {
            Validation.SetErrors(result.Errors);
        }
        else
        {
            AllCategories = (await UseCases.GetCategoryListAsync()).Categories;
            await CloseCategoryModal();
            Validation.SetSuccess(IsEditing ? "Category updated successfully." : "Category created successfully.");
        }
    }

    private async Task OnCategoryDelete()
    {
        Validation.Clear();
        if (EditCategory is null) return;

        var result = await UseCases.DeleteCategoryAsync(EditCategory.Id);

        if (!result.IsSuccess)
        {
            Validation.SetErrors(result.Errors);
        }
        else
        {
            AllCategories = (await UseCases.GetCategoryListAsync()).Categories;
            await CloseCategoryModal();
            Validation.SetSuccess("Category deleted successfully.");
        }
    }

    private async Task OnGroupSave()
    {
        Validation.Clear();
        if (EditGroup is null) return;

        var result = await UseCases.SaveCategoryGroupEditAsync(EditGroup);
        if (!result.IsSuccess)
        {
            Validation.SetErrors(result.Errors);
        }
        else
        {
            CategoryGroups = (await UseCases.GetCategoryListAsync()).Groups;
            CurrentGroup = CategoryGroups.FirstOrDefault(g => g.Id == EditGroup.Id);
            await CloseGroupModal();
            Validation.SetSuccess(IsEditing ? "Area updated successfully." : "Area created successfully.");
        }
    }

    private async Task OnGroupDelete()
    {
        Validation.Clear();
        if (EditGroup is null) return;

        var result = await UseCases.DeleteCategoryGroupAsync(EditGroup.Id);
        if (!result.IsSuccess)
        {
            Validation.SetErrors(result.Errors);
        }
        else
        {
            CategoryGroups = (await UseCases.GetCategoryListAsync()).Groups;
            CurrentGroup = null;
            await CloseGroupModal();
            Validation.SetSuccess("Area deleted successfully.");
        }
    }

    private async Task OpenCategoryCreateModal()
    {
        EditCategory = new CategorySummary { Id = Guid.NewGuid(), Name = string.Empty, Colour = string.Empty, GroupId = CurrentGroup?.Id ?? Guid.Empty };
        IsEditing = false;
        await CategoryModal.ShowAsync();
    }

    private async Task OpenCategoryEditModal(CategorySummary category)
    {
        EditCategory = category;
        IsEditing = true;
        await CategoryModal.ShowAsync();
    }

    private async Task OpenGroupCreateModal()
    {
        EditGroup = new CategoryGroupSummary { Id = Guid.NewGuid(), Name = string.Empty, Colour = string.Empty, IsIncome = false };
        IsEditing = false;
        await GroupModal.ShowAsync();
    }

    private async Task OpenGroupEditModal()
    {
        if (CurrentGroup is null) return;

        EditGroup = new() { Id = CurrentGroup.Id, Name = CurrentGroup.Name, Colour = CurrentGroup.Colour, IsIncome = CurrentGroup.IsIncome };
        IsEditing = true;
        await GroupModal.ShowAsync();
    }

    private async Task CloseCategoryModal()
    {
        await CategoryModal.HideAsync();
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
