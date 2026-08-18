namespace FinanceManager.WebApp.Components.Pages.Features.Budget;

using FinanceManager.Application.Constants.Navigation;
using FinanceManager.Application.DTOs;
using FinanceManager.Application.Enums;
using FinanceManager.Domain.Enums;
using FinanceManager.WebApp.Components.Base;
using FinanceManager.WebApp.Components.Shared.Budget;
using Microsoft.AspNetCore.Components;

[Route(Pages.Budget)]
public partial class _Page : MainPageBase
{
    private IReadOnlyList<BudgetColumn> Columns { get; set; } = [];
    private IReadOnlyList<CategorySummary> Categories { get; set; } = [];
    private bool HideEmptyCategories { get; set; }

    private BudgetYearSummary? ActiveBudgetYear { get; set; } 
    private IEnumerable<int> AvailableYears { get; set; } = [];

    private decimal TotalBudget { get; set; }
    private decimal TotalIncome { get; set; }
    private decimal TotalExpense { get; set; }

    private bool IsEditing { get; set; }
    private BudgetCell? CurrentCell { get; set; }
    private BudgetColumn? CurrentColumn { get; set; } 
    private BudgetCellEntry? CurrentEntry { get; set; }
    private BudgetYearSummary? CurrentYear { get; set; }

    private BudgetGridMode EntryTypeFilter { get; set; } = BudgetGridMode.Net;

    private BudgetCellModal BudgetCellModal { get; set; } = new();
    private BudgetYearModal BudgetYearModal { get; set; } = new();
    private BudgetEntryModal BudgetEntryModal { get; set; } = new();

    private bool HasPopulatedCategories => Columns.Any(c => c.Cells.Count != 0);

    private readonly IReadOnlyList<BudgetScope> BudgetScopes = [BudgetScope.Monthly, BudgetScope.Fortnightly, BudgetScope.Weekly];

    private static readonly BudgetGridMode[] EntryTypeFilters =
    [
        BudgetGridMode.Net,
        BudgetGridMode.Income,
        BudgetGridMode.Expense
    ];

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        await ReloadAsync(DateTime.Now.Year);
        HideEmptyCategories = HasPopulatedCategories && await Preferences.HideEmptyBudgetCategories;
    }

    private async Task OnRangeSelected(int index)
    {
        await ReloadAsync(AvailableYears.ElementAtOrDefault(index));
    }

    private async Task OnEntryTypeFilterChanged(BudgetGridMode mode)
    {
        EntryTypeFilter = mode;
        await ReloadAsync(DateTime.Now.Year);
    }

    private async Task OnHideEmptyCategoriesChanged(bool value)
    {
        HideEmptyCategories = value;
        await Preferences.Set(PreferenceNames.HideEmptyBudgetCategories, value);
    }

    private async Task ReloadAsync(int year)
    {
        AvailableYears = (await UseCases.GetBudgetYearsAsync()).AvailableYears.OrderByDescending(y => y);

        var page = await UseCases.GetPagedBudgetAsync(year, EntryTypeFilter);

        Columns = page.Columns;
        Categories = page.Categories;
        ActiveBudgetYear = page.BudgetYear;
        TotalBudget = page.TotalBudget;
        TotalIncome = page.TotalIncome;
        TotalExpense = page.TotalExpense;
    }
    
    private async Task OpenCellModal(BudgetCell cell, BudgetColumn column)
    {
        CurrentCell = cell;
        CurrentColumn = column;
        await BudgetCellModal.ShowAsync();
    }

    private async Task CloseCellModal()
    {
        await BudgetCellModal.HideAsync();
    }

    private async Task CreateEntry(BudgetCell cell, BudgetColumn column)
    {
        Validation.Clear();
        CurrentEntry = new() { ScopePosition = Columns.ToList().IndexOf(column), Length = 1, Category = cell.Category };
        IsEditing = false;
        await BudgetEntryModal.ShowAsync();
    }

    private async Task EditEntry(BudgetCellEntry entry)
    {
        Validation.Clear();
        CurrentEntry = entry;
        IsEditing = true;
        await BudgetEntryModal.ShowAsync();
    }

    private async Task CreateYear()
    {
        Validation.Clear();
        CurrentYear = new() { Year = AvailableYears.Any() ? AvailableYears.Max() + 1 : DateTime.Now.Year, Scope = BudgetScope.Monthly };
        IsEditing = false;
        await BudgetYearModal.ShowAsync();
    }

    private async Task EditYear()
    {
        Validation.Clear();
        CurrentYear = ActiveBudgetYear;
        IsEditing = true;
        await BudgetYearModal.ShowAsync();
    }

    private async Task SaveEntry()
    {
        if (CurrentEntry is null || ActiveBudgetYear is null) return;

        Validation.Clear();

        if (string.IsNullOrEmpty(CurrentEntry.Name))
        {
            Validation.SetError("Name is required");
            return;
        }
        if (CurrentEntry.Category is null)
        {
            Validation.SetError("Category is required");
            return;
        }
        if (CurrentEntry.Amount <= 0)
        {
            Validation.SetError("Amount is required");
            return;
        }

        try
        {
            await UseCases.SaveBudgetEntryAsync(CurrentEntry, ActiveBudgetYear.Year, IsEditing);
            if (IsEditing)
            {
                Validation.SetSuccess("Budget entry updated");
            }
            else
            {
                Validation.SetSuccess("Budget entry created");
            }

            await BudgetEntryModal.HideAsync();
            await ReloadAsync(ActiveBudgetYear.Year);
            await BudgetCellModal.ShowAsync();
        }
        catch (Exception ex)
        {
            Validation.SetError(ex.Message);
        }
    }

    private async Task DeleteEntry()
    {
        if (CurrentEntry is null || CurrentEntry.EntityId is null) return;

        try
        {
            await UseCases.DeleteBudgetEntryAsync(CurrentEntry.EntityId.Value);
            CurrentCell?.BudgetEntries = CurrentCell.BudgetEntries.Where(e => e.EntityId != CurrentEntry.EntityId);
            Validation.SetSuccess("Budget entry deleted");
            await BudgetEntryModal.HideAsync();
            await ReloadAsync(DateTime.Now.Year);
            await BudgetCellModal.ShowAsync();
        }
        catch (Exception ex)
        {
            Validation.SetError(ex.Message);
        }
    }

    private async Task CancelEntry()
    {
        await BudgetEntryModal.HideAsync();
        await BudgetCellModal.ShowAsync();
    }

    private async Task SaveBudget()
    {
        if (CurrentYear is null) return;

        Validation.Clear();

        try
        {
            var saveResult = await UseCases.SaveBudgetAsync(CurrentYear.Year, CurrentYear.Scope, IsEditing);
            if (saveResult.Errors.Any())
            {
                Validation.SetErrors(saveResult.Errors);
                return;
            }
            Validation.SetSuccess($"Budget {(IsEditing ? "updated" : "created")} successfully");
            await BudgetYearModal.HideAsync();
            await ReloadAsync(CurrentYear.Year);
        }
        catch (Exception ex)
        {
            Validation.SetError(ex.Message);
        }
    }

    private async Task DeleteBudget()
    {
        if (CurrentYear is null) return;
        try
        {
            await UseCases.DeleteBudgetAsync(CurrentYear.EntityId);
            Validation.SetSuccess("Budget deleted successfully");
            await BudgetYearModal.HideAsync();
            await ReloadAsync(DateTime.Now.Year);
        }
        catch (Exception ex)
        {
            Validation.SetError(ex.Message);
        }
    }

    private async Task CancelBudget()
    {
        await BudgetYearModal.HideAsync();
    }
}
