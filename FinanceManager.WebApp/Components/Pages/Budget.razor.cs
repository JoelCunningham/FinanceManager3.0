namespace FinanceManager.WebApp.Components.Pages;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Enums;
using FinanceManager.Domain.Entities;

public partial class Budget : PageBase
{
    public IReadOnlyList<BudgetCell> Cells { get; set; } = [];
    public IReadOnlyList<CategorySummary> Categories { get; set; } = [];
    public BudgetYear? CurrentBudgetYear { get; set; } = null;
    public int CurrentYear { get; set; } = DateTime.Now.Year;

    public BudgetCellEntry? CurrentEntry { get; set; }
    public bool IsEditing { get; set; }
    public BudgetGridMode EntryTypeFilter { get; set; } = BudgetGridMode.Net;

    private object? _budgetEntryModal;

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
    }

    public async Task Prev() => await ReloadAsync(CurrentYear - 1);
    public async Task Next() => await ReloadAsync(CurrentYear + 1);
    public async Task Today() => await ReloadAsync(DateTime.Now.Year);

    public async Task OnEntryTypeFilterChanged(BudgetGridMode mode)
    {
        EntryTypeFilter = mode;
        await ReloadAsync(CurrentYear);
    }

    public async Task OpenEditModal() => await InvokeBudgetEntryModalAsync("ShowAsync");
    public async Task CloseEditModal() => await InvokeBudgetEntryModalAsync("HideAsync");

    private async Task ReloadAsync(int year)
    {
        var page = await UseCases.GetPagedBudgetAsync(year, EntryTypeFilter);

        Cells = page.Cells;
        Categories = page.Categories;
        CurrentBudgetYear = page.BudgetYear;
        CurrentYear = year;
    }

    public async Task CreateEntry(BudgetCell cell)
    {
        Validation.Clear();
        CurrentEntry = new BudgetCellEntry { OverallScopePosition = cell.Index, OverallLength = 1 };
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

    public async Task Save()
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

    public async Task Delete()
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

    private Task InvokeBudgetEntryModalAsync(string method)
    {
        if (_budgetEntryModal is null) return Task.CompletedTask;
        return method switch
        {
            "ShowAsync" => ((dynamic)_budgetEntryModal).ShowAsync(),
            "HideAsync" => ((dynamic)_budgetEntryModal).HideAsync(),
            _ => Task.CompletedTask
        };
    }
}
