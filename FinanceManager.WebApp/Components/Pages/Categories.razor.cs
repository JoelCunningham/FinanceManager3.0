namespace FinanceManager.WebApp.Components.Pages;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Enums;
using FinanceManager.Application.UseCases;
using FinanceManager.WebApp.Components.Base;
using FinanceManager.WebApp.Components.Features.Categories;
using FinanceManager.WebApp.Components.Features.Categories.Group;
using FinanceManager.WebApp.Components.Shared.Wrappers;
using FinanceManager.WebApp.Models;
using FinanceManager.WebApp.Navigation;
using FinanceManager.WebApp.Utilities;
using Microsoft.AspNetCore.Components;

[Route(Pages.Categories)]
[Route(Pages.Categories + Tabs.GroupName)]
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
    public CategoryPeriodDetails? CurrentCategory { get; set; } = null;

    public CategoryModal CategoryModal { get; set; } = new();
    public CategoryGroupModal GroupModal { get; set; } = new();
    public TransactionsModal TransactionsModal { get; set; } = new();

    public DataGridModel<FilterQuery, TransactionSummary> TransactionData { get; set; } = new(20);
    public int PeriodOffset { get; set; } = 0;

    public Confirmation Confirmation { get; set; } = new();
    public string ConfriamtionMessage { get; set; } = string.Empty;
    public Action? ConfirmationAction { get; set; } = null;

    public IEnumerable<CategoryGroupSummary> FilteredGroups => CategoryGroups
        .Where(g => string.IsNullOrWhiteSpace(GroupSearch) || g.Name.Contains(GroupSearch, StringComparison.OrdinalIgnoreCase))
        .Where(g => GroupMode == CategoryGroupMode.All || (GroupMode == CategoryGroupMode.Expense && !g.IsIncome) || (GroupMode == CategoryGroupMode.Income && g.IsIncome))
        .OrderByDescending(g => g.IsIncome).ThenBy(g => g.Name);

    protected override async Task OnInitializedAsync()
    {
        Validation.Messenger = Messenger;

        await ResfeshCategories();

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

    private async Task ResfeshCategories()
    {
        var allCategories = await UseCases.GetCategoriesAsync();
        CategoryGroups = allCategories.Groups;
        AllCategories = allCategories.Categories;
        if (CurrentGroup is not null)
        {
            CurrentGroup = (await UseCases.GetCategoryGroupDetailsAsync(CurrentGroup.Id, PeriodOffset)).GroupDetails;
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
            await ResfeshCategories();
            await CloseCategoryModal();
            Validation.SetSuccess(IsEditing ? "Category updated successfully." : "Category created successfully.");
        }
    }

    private async Task OnCategoryDelete()
    {
        Validation.Clear();
        if (EditCategory is null) return;

        string? confrimationMessage;
        if (EditCategory.TotalTransactions > 0)
        {
            confrimationMessage = $"This category has {EditCategory.TotalTransactions} {LanguageUtilities.Pluralise("transactions", EditCategory.TotalTransactions)} associated with it. Deleting this category will unassign {LanguageUtilities.Pluralise("them", EditCategory.TotalTransactions)} and delete any associated budgets. Are you sure you want to delete this category?";
        }
        else
        {
            confrimationMessage = "This category has no associated transactions. Are you sure you want to delete this category?"; //TODO fix messages
        }

        if (await Confirmation.Show(confrimationMessage))
        {
            var result = await UseCases.DeleteCategoryAsync(EditCategory.Id);

            if (!result.IsSuccess)
            {
                Validation.SetErrors(result.Errors);
            }
            else
            {
                await ResfeshCategories();
                await CloseCategoryModal();
                Validation.SetSuccess("Category deleted successfully.");
            }
        }
        else
        {
            await CategoryModal.ShowAsync();
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
            await ResfeshCategories();
            await CloseGroupModal();
            Validation.SetSuccess(IsEditing ? "Area updated successfully." : "Area created successfully.");
        }
    }

    private async Task OnGroupDelete()
    {
        Validation.Clear();
        if (EditGroup is null) return;

        string? confrimationMessage;
        if (EditGroup.CategoryCount > 0)
        {
            confrimationMessage = $"This area has {EditGroup.CategoryCount} {LanguageUtilities.Pluralise("categories", EditGroup.CategoryCount)} associated with it. Deleting this area will delete {LanguageUtilities.Pluralise("them", EditGroup.CategoryCount)}, and unassign any associated transactions. Are you sure you want to delete this area?";
        }
        else
        {
            confrimationMessage = "This area has no associated categories. Are you sure you want to delete this area?";
        }

        if (await Confirmation.Show(confrimationMessage))
        {

            var result = await UseCases.DeleteCategoryGroupAsync(EditGroup.Id);
            if (!result.IsSuccess)
            {
                Validation.SetErrors(result.Errors);
            }
            else
            {
                CurrentGroup = null;
                Navigation.NavigateTo($"{Pages.Categories}");

                await ResfeshCategories();
                await CloseGroupModal();
                Validation.SetSuccess("Area deleted successfully.");
            }
        }
        else
        {
            await GroupModal.ShowAsync();
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
        await TransactionsModal.HideAsync();
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
