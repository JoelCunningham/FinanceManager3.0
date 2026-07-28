namespace FinanceManager.WebApp.Components.Pages.Features.CategoryGroup;

using FinanceManager.Application.Constants.Navigation;
using FinanceManager.Application.DTOs;
using FinanceManager.Application.UseCases;
using FinanceManager.Domain.Enums;
using FinanceManager.WebApp.Components.Base;
using FinanceManager.WebApp.Components.Shared.Modals;
using FinanceManager.WebApp.Components.Shared.Wrappers;
using FinanceManager.WebApp.Models;
using FinanceManager.WebApp.Utilities;
using Microsoft.AspNetCore.Components;

[Route(Pages.Categories + Tabs.GroupName)]
public partial class _Page : MainPageBase
{
    [Parameter] public string GroupName { get; set; } = default!;

    private CategoryGroupDetails Group { get; set; } = default!;
    private DataGridModel<FilterQuery, TransactionSummary> TransactionData { get; set; } = new(20);

    private CategoryModal CategoryModal { get; set; } = new();
    private CategoryGroupModal GroupModal { get; set; } = new();
    private TransactionsModal TransactionsModal { get; set; } = new();
    private Confirmation Confirmation { get; set; } = new();

    private bool IsEditing { get; set; } = false;
    private CategoryGroupSummary EditGroup { get; set; } = default!;

    private CategoryPeriodDetails EditCategory { get; set; } = default!;
    private CategoryPeriodDetails CurrentCategory { get; set; } = default!;

    private IEnumerable<ScopedPeriod> AvailablePeriods { get; set; } = [];
    private int CurrentPeriodIndex { get; set; } = 0;
    private BudgetScope TodayScope => AvailablePeriods.FirstOrDefault(p => p.Includes(DateOnly.FromDateTime(DateTime.Today)))?.Scope ?? BudgetScope.Monthly;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        AvailablePeriods = (await UseCases.GetAvailablePeriodsAsync()).Periods;
        TransactionData.GetDataFunc = async (query) => (await UseCases.GetPagedTransactionsAsync(query)).Page;
    }

    protected override async Task OnParametersSetAsync()
    {
        await RefreshGroup();
    }

    private async Task RefreshGroup()
    {
        try
        {
            var currentPeriod = AvailablePeriods.ElementAtOrDefault(CurrentPeriodIndex) ?? new ScopedPeriod(BudgetScope.Monthly, DateOnly.FromDateTime(DateTime.Today), 0);
            Group = (await UseCases.GetCategoryGroupDetailsAsync(GroupName, currentPeriod)).GroupDetails;
        }
        catch
        {
            Navigation.NavigateTo(Pages.NotFound, true);
        }
    }

    private async Task OpenGroupModal()
    {
        EditGroup = new() { Id = Group.Id, Name = Group.Name, Colour = Group.Colour, Icon = Group.Icon, IsIncome = Group.IsIncome };
        IsEditing = true;
        await GroupModal.ShowAsync();
    }

    private async Task CloseGroupModal()
    {
        await GroupModal.HideAsync();
    }

    private async Task OpenCategoryCreateModal()
    {
        EditCategory = new CategoryPeriodDetails { Id = Guid.NewGuid(), Name = string.Empty, Colour = string.Empty, GroupId = Group.Id };
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

    private async Task CloseCategoryModal()
    {
        await CategoryModal.HideAsync();
    }

    private async Task OpenTransactionsModal(CategoryPeriodDetails category)
    {
        CurrentCategory = category;
        TransactionData.Query.FilterCategory = CategorySummary.FromCategory(category.ToCategory());
        await TransactionsModal.ShowAsync();
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
            await RefreshGroup();
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
                await RefreshGroup();
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
            await RefreshGroup();
            await CloseGroupModal();
            Validation.SetSuccess("Area updated successfully.");
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
                await CloseGroupModal();
                Validation.SetSuccess("Area deleted successfully.");
                Navigation.NavigateTo($"{Pages.Categories}");
            }
        }
        else
        {
            await GroupModal.ShowAsync();
        }
    }

    private async Task SetCurrentPeriod(int index)
    {
        CurrentPeriodIndex = index;

        if (Group is not null)
        {
            await RefreshGroup();
        }
    }
}