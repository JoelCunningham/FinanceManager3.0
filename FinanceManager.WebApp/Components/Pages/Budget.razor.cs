namespace FinanceManager.WebApp.Components.Pages;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Enums;
using FinanceManager.Domain.Entities;
using FinanceManager.Domain.Enums;
using FinanceManager.WebApp.Components.Base;
using FinanceManager.WebApp.Components.Features.Budget;

public partial class Budget : PageBase
{
    public IReadOnlyList<BudgetCell> Cells { get; set; } = [];
    public IReadOnlyList<CategorySummary> Categories { get; set; } = [];
    public bool HideEmptyCategories { get; set; }

    public BudgetYear? CurrentBudgetYear { get; set; } = null;
    public int CurrentYear { get; set; } = DateTime.Now.Year;
    public decimal TotalBudget { get; set; }
    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }

    public BudgetCellEntry? CurrentEntry { get; set; }
    public bool IsEditing { get; set; }

    public BudgetGridMode EntryTypeFilter { get; set; } = BudgetGridMode.Net;
    public IReadOnlyList<BudgetScope> BudgetScopes = [BudgetScope.Monthly, BudgetScope.Fortnightly, BudgetScope.Weekly];

    private BudgetYearModal BudgetYearModal = new();
    private BudgetEntryModal BudgetEntryModal = new();

    public bool HasPopulatedCategories => Cells.Any(c => c.Entries.Count != 0);

    private static readonly BudgetGridMode[] EntryTypeFilters =
    [
        BudgetGridMode.Net,
        BudgetGridMode.Income,
        BudgetGridMode.Expense
    ];

    protected override async Task OnInitializedAsync()
    {
        Validation.Messenger = Messenger;
        await ReloadAsync(DateTime.Now.Year);
        HideEmptyCategories = HasPopulatedCategories && await Preferences.HideEmptyBudgetCategories;
    }

    public async Task Prev() => await ReloadAsync(CurrentYear - 1);
    public async Task Next() => await ReloadAsync(CurrentYear + 1);
    public async Task Today() => await ReloadAsync(DateTime.Now.Year);

    public async Task OnEntryTypeFilterChanged(BudgetGridMode mode)
    {
        EntryTypeFilter = mode;
        await ReloadAsync(CurrentYear);
    }

    public async Task OnHideEmptyCategoriesChanged(bool value)
    {
        HideEmptyCategories = value;
        await Preferences.Set(PreferenceNames.HideEmptyBudgetCategories, value);
    }

    public async Task OpenEditModal() => await BudgetEntryModal.ShowAsync();
    public async Task CloseEditModal() => await BudgetEntryModal.HideAsync();
    public async Task OpenCreateModal() => await BudgetYearModal.ShowAsync();
    public async Task CloseCreateModal() => await BudgetYearModal.HideAsync();

    private async Task ReloadAsync(int year)
    {
        var page = await UseCases.GetPagedBudgetAsync(year, EntryTypeFilter);

        Cells = page.Cells;
        Categories = page.Categories;
        CurrentBudgetYear = page.BudgetYear;
        TotalBudget = page.TotalBudget;
        TotalIncome = page.TotalIncome;
        TotalExpense = page.TotalExpense;
        CurrentYear = year;
    }

    public async Task CreateEntry(BudgetCell cell, CategorySummary category)
    {
        Validation.Clear();
        CurrentEntry = new BudgetCellEntry { OverallScopePosition = cell.Index, OverallLength = 1, Category = category };
        IsEditing = false;
        await OpenEditModal();
    }

    public async Task EditEntry(BudgetCellEntry entry)
    {
        Validation.Clear();
        CurrentEntry = entry;
        IsEditing = true;
        await OpenEditModal();
    }

    public async Task CreateYear()
    {
        Validation.Clear();
        IsEditing = false;
        await OpenCreateModal();
    }

    public async Task EditYear()
    {
        Validation.Clear();
        IsEditing = true;
        await OpenCreateModal();
    }

    public async Task SaveEntry()
    {
        if (CurrentEntry is null || CurrentBudgetYear is null) return;

        Validation.Clear();

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
            await UseCases.SaveBudgetEntryAsync(CurrentEntry, CurrentBudgetYear.Year, IsEditing);
            if (IsEditing)
            {
                Validation.SetSuccess("Budget entry updated");
            }
            else
            {
                Validation.SetSuccess("Budget entry created");
            }

            await CloseEditModal();
            await ReloadAsync(CurrentBudgetYear.Year);
        }
        catch (Exception ex)
        {
            Validation.SetError(ex.Message);
        }
    }

    public async Task DeleteEntry()
    {
        if (CurrentEntry is null || CurrentEntry.EntityId is null) return;

        try
        {
            await UseCases.DeleteBudgetEntryAsync(CurrentEntry.EntityId.Value);
            Validation.SetSuccess("Budget entry deleted");
            await CloseEditModal();
            await ReloadAsync(CurrentYear);
        }
        catch (Exception ex)
        {
            Validation.SetError(ex.Message);
        }
    }

    public async Task SaveBudget(int year, BudgetScope scope)
    {
        Validation.Clear();

        try
        {
            var saveResult = await UseCases.SaveBudgetAsync(year, scope, IsEditing);
            if (saveResult.Errors.Any())
            {
                Validation.SetErrors(saveResult.Errors);
                return;
            }
            Validation.SetSuccess($"Budget {(IsEditing ? "updated" : "created")} successfully");
            await CloseCreateModal();
            await ReloadAsync(year);
        }
        catch (Exception ex)
        {
            Validation.SetError(ex.Message);
        }
    }
}
