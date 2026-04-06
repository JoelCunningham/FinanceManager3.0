namespace FinanceManager.WebApp.Components.Pages;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.UseCases;
using FinanceManager.Domain.Entities;
using FinanceManager.WebApp.Models;
using Havit.Blazor.Components.Web;
using Microsoft.AspNetCore.Components;

public partial class Budget : ComponentBase
{
    [Inject] public BudgetWorkflow Workflow { get; set; } = default!;
    [Inject] public IHxMessengerService Messenger { get; set; } = default!;

    public ValidationModel Validation { get; set; } = new();

    public IReadOnlyList<BudgetCell> Cells { get; set; } = [];
    public IReadOnlyList<CategorySummary> Categories { get; set; } = [];
    public BudgetPeriod? CurrentPeriod { get; set; } = null;

    public BudgetCellEntry? CurrentEntry { get; set; }
    public bool IsEditing { get; set; }

    public int CurrentYear => CurrentPeriod?.Year ?? DateTime.Now.Year;

    private object? _budgetEntryModal;

    protected override async Task OnInitializedAsync()
    {
        Validation.Messenger = Messenger;
        await ReloadAsync(DateTime.Now.Year);
    }

    public async Task Prev() => await ReloadAsync(CurrentYear - 1);
    public async Task Next() => await ReloadAsync(CurrentYear + 1);
    public async Task Today() => await ReloadAsync(DateTime.Now.Year);

    public async Task OpenEditModal() => await InvokeBudgetEntryModalAsync("ShowAsync");
    public async Task CloseEditModal() => await InvokeBudgetEntryModalAsync("HideAsync");

    private async Task ReloadAsync(int year)
    {
        var page = await Workflow.GetPageAsync(year);

        Cells = page.Cells;
        Categories = page.Categories;
        CurrentPeriod = page.BudgetPeriod;
    }

    public async Task CreateEntry(BudgetCell cell)
    {
        Validation.Clear();
        CurrentEntry = new BudgetCellEntry { OverallPeriodPosition = cell.Index, OverallLength = 1 };
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
        if (CurrentEntry is null || CurrentPeriod is null) return;

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
            await Workflow.SaveAsync(CurrentEntry, CurrentPeriod.Year, IsEditing);
            if (IsEditing)
            {
                Validation.SetSuccess("Budget entry updated");
            }
            else
            {
                Validation.SetSuccess("Budget entry created");
            }

            await CloseEditModal();
            await ReloadAsync(CurrentPeriod.Year);
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
            await Workflow.DeleteAsync(CurrentEntry.EntityId.Value);
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
