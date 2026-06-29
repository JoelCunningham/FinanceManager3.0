namespace FinanceManager.WebApp.Components.Pages;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Enums;
using FinanceManager.Application.UseCases;
using FinanceManager.WebApp.Components.Base;
using FinanceManager.WebApp.Components.Features.Categories;
using FinanceManager.WebApp.Components.Features.Categories.Group;
using FinanceManager.WebApp.Enums;
using FinanceManager.WebApp.Models;
using Microsoft.AspNetCore.Components;

public partial class Categories : PageBase
{
    [Parameter] public string? GroupName { get; set; }

    public IReadOnlyList<CategorySummary> AllCategories { get; set; } = [];
    public IReadOnlyList<CategoryGroupSummary> CategoryGroups { get; set; } = [];

    public string GroupSearch { get; set; } = string.Empty;
    public CategoryGroupMode GroupMode { get; set; } = CategoryGroupMode.All;

    public bool IsEditing { get; set; } = false;

    public CategoryGroupSummary? EditGroup { get; set; } = null;
    public CategoryPeriodDetails? EditCategory { get; set; } = null;

    public CategoryGroupDetails? CurrentGroup { get; set; } = null;
    public CategorySummary? CurrentCategory { get; set; } = null;

    public CategoryModal CategoryModal { get; set; } = new();
    public CategoryGroupModal GroupModal { get; set; } = new();
    public TransactionsModal TransactionsModal { get; set; } = new();

    public DataGridModel<FilterQuery, TransactionSummary> TransactionData { get; set; } = new(20);
    public int PeriodOffset { get; set; } = 0;

    public IEnumerable<CategoryGroupSummary> FilteredGroups => CategoryGroups
        .Where(g => string.IsNullOrWhiteSpace(GroupSearch) || g.Name.Contains(GroupSearch, StringComparison.OrdinalIgnoreCase))
        .Where(g => GroupMode == CategoryGroupMode.All || (GroupMode == CategoryGroupMode.Expense && !g.IsIncome) || (GroupMode == CategoryGroupMode.Income && g.IsIncome))
        .OrderByDescending(g => g.IsIncome).ThenBy(g => g.Name);

    protected override async Task OnInitializedAsync()
    {
        Validation.Messenger = Messenger;

        var allCategories = await UseCases.GetCategoriesAsync();
        CategoryGroups = allCategories.Groups;
        AllCategories = allCategories.Categories;

        TransactionData.GetDataFunc = async (query) => (await UseCases.GetPagedTransactionsAsync(query)).Page;
    }

    protected override async Task OnParametersSetAsync()
    {
        CurrentGroup = null;
        if (GroupName is not null)
        {
            var groupId = CategoryGroups.FirstOrDefault(g => g.Name == GroupName)?.Id;
            if (groupId is not null)
            {
                CurrentGroup = (await UseCases.GetCategoryGroupDetailsAsync(groupId.Value, PeriodOffset)).GroupDetails;
            }
        }
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
            AllCategories = (await UseCases.GetCategoriesAsync()).Categories;
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
            AllCategories = (await UseCases.GetCategoriesAsync()).Categories;
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
            CategoryGroups = (await UseCases.GetCategoriesAsync()).Groups;
            GroupName = EditGroup.Name;
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
            CategoryGroups = (await UseCases.GetCategoriesAsync()).Groups;
            GroupName = null;
            await CloseGroupModal();
            Validation.SetSuccess("Area deleted successfully.");
        }
    }

    private async Task OpenCategoryCreateModal()
    {
        EditCategory = new CategoryPeriodDetails { Id = Guid.NewGuid(), Name = string.Empty, Colour = string.Empty, GroupId = CurrentGroup?.Id ?? Guid.Empty };
        IsEditing = false;
        await CategoryModal.ShowAsync();
    }

    private async Task OpenCategoryEditModal(CategoryPeriodDetails category)
    {
        EditCategory = category;
        IsEditing = true;
        await CategoryModal.ShowAsync();
    }

    private async Task OpenGroupCreateModal()
    {
        EditGroup = CategoryGroupSummary.Empty(Guid.NewGuid());
        IsEditing = false;
        await GroupModal.ShowAsync();
    }

    private async Task OpenGroupEditModal()
    {
        if (CurrentGroup is null) return;

        EditGroup = new() { Id = CurrentGroup.Id, Name = CurrentGroup.Name, Colour = CurrentGroup.Colour, Icon = CurrentGroup.Icon, IsIncome = CurrentGroup.IsIncome };
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
        Navigation.NavigateTo($"{Pages.Categories}/{group.Name}");
    }

    private void CloseGroupPanel()
    {
        Navigation.NavigateTo($"{Pages.Categories}");
    }

    private async Task OpenTransactionsModal(CategoryPeriodDetails category)
    {
        CurrentCategory = category;
        TransactionData.Query.FilterCategory = CategorySummary.FromCategory(category.ToCategory());
        await TransactionsModal.ShowAsync();
    }

    private async Task AdjustOffset(int offset)
    {
        if (offset == 0)
        {
            PeriodOffset = 0;
        }
        else
        {
            PeriodOffset += offset;
        }

        if (CurrentGroup is not null)
        {
            CurrentGroup = (await UseCases.GetCategoryGroupDetailsAsync(CurrentGroup.Id, PeriodOffset)).GroupDetails;
        }
    }
}
