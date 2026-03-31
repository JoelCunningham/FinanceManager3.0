namespace FinanceManager.WebApp.Components.Pages;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.UseCases.Budget;
using FinanceManager.Application.UseCases;
using FinanceManager.Domain.Constants;
using FinanceManager.Domain.Entities;
using FinanceManager.Domain.Enums;
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
    public ScopedPeriod GridPeriod { get; set; } = new(BudgetScope.Monthly, TodayDate, DateConstants.MONTHS_IN_YEAR);

    private object? _budgetEntryModal;
    public EditBudgetEntryModel? EditModel { get; set; }
    public CategorySummary? SelectedCategory { get; set; }
    public bool IsEditing { get; set; }
    public string EditModalTitle => IsEditing ? "Edit Budget Entry" : "New Budget Entry";

    public static DateOnly TodayDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    protected override async Task OnInitializedAsync()
    {
        Validation.Messenger = Messenger;
        await ReloadAsync();
    }

    public async Task SetScope(ScopedPeriod period)
    {
        GridPeriod = period;
        await ReloadAsync();
    }

    public async Task Prev()
    {
        GridPeriod = GridPeriod.Scope switch
        {
            BudgetScope.Monthly => new ScopedPeriod(GridPeriod.Scope, GridPeriod.StartDate.AddYears(-1), DateConstants.MONTHS_IN_YEAR),
            BudgetScope.Weekly => new ScopedPeriod(GridPeriod.Scope, GridPeriod.StartDate.AddDays(-DateConstants.DAYS_IN_WEEK)),
            BudgetScope.Fortnightly => new ScopedPeriod(GridPeriod.Scope, GridPeriod.StartDate.AddDays(-DateConstants.DAYS_IN_FORTNIGHT)),
            _ => GridPeriod
        };
        await ReloadAsync();
    }

    public async Task Next()
    {
        GridPeriod = GridPeriod.Scope switch
        {
            BudgetScope.Monthly => new ScopedPeriod(GridPeriod.Scope, GridPeriod.StartDate.AddYears(1), DateConstants.MONTHS_IN_YEAR),
            BudgetScope.Weekly => new ScopedPeriod(GridPeriod.Scope, GridPeriod.StartDate.AddDays(DateConstants.DAYS_IN_WEEK)),
            BudgetScope.Fortnightly => new ScopedPeriod(GridPeriod.Scope, GridPeriod.StartDate.AddDays(DateConstants.DAYS_IN_FORTNIGHT)),
            _ => GridPeriod
        };
        await ReloadAsync();
    }

    public async Task Today()
    {
        GridPeriod = GridPeriod.Scope switch
        {
            BudgetScope.Monthly => new ScopedPeriod(GridPeriod.Scope, TodayDate, DateConstants.MONTHS_IN_YEAR),
            BudgetScope.Weekly => new ScopedPeriod(GridPeriod.Scope, TodayDate, DateConstants.DAYS_IN_WEEK),
            BudgetScope.Fortnightly => new ScopedPeriod(GridPeriod.Scope, TodayDate, DateConstants.DAYS_IN_FORTNIGHT),
            _ => GridPeriod
        };
        await ReloadAsync();
    }

    private async Task ReloadAsync()
    {
        var page = await Workflow.GetPageAsync(GridPeriod);
        Cells = page.Cells;
        Categories = page.Categories;
    }

    public async Task OnCellClicked(BudgetCell cell)
    {
        Validation.Clear();

        SelectedCategory = Categories.FirstOrDefault();
        EditModel = new EditBudgetEntryModel
        {
            Id = null,
            Amount = 0m,
            Notes = null,
            CategoryId = SelectedCategory?.Id ?? Guid.Empty,
            StartCellIndex = cell.Index,
            Span = 1
        };

        IsEditing = false;
        await InvokeBudgetEntryModalAsync("ShowAsync");
    }

    public async Task EditEntry(BudgetEntry entry)
    {
        Validation.Clear();

        SelectedCategory = Categories.FirstOrDefault(c => c.Id == entry.CategoryId);
        EditModel = new EditBudgetEntryModel
        {
            Id = entry.Id,
            Amount = entry.Amount,
            Notes = entry.Notes,
            CategoryId = entry.CategoryId,
            StartCellIndex = GetBudgetPage.GetCellIndexForEntry(GridPeriod, entry),
        };

        IsEditing = true;
        await InvokeBudgetEntryModalAsync("ShowAsync");
    }

    public async Task CloseEditModal()
    {
        await InvokeBudgetEntryModalAsync("HideAsync");
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

    public Task OnSelectedCategoryChanged(CategorySummary? value)
    {
        SelectedCategory = value;
        EditModel?.CategoryId = value?.Id ?? Guid.Empty;

        return Task.CompletedTask;
    }

    public async Task Save()
    {
        if (EditModel is null) return;

        Validation.Clear();

        if (EditModel.CategoryId == Guid.Empty)
        {
            Validation.SetError("Category is required");
            return;
        }

        if (EditModel.Amount == 0m)
        {
            Validation.SetError("Amount is required");
            return;
        }

        if (GridPeriod.Scope != BudgetScope.Monthly)
        {
            EditModel.Span = 1;
        }

        var period = new BudgetPeriod
        {
            Id = Guid.NewGuid(),
            Scope = GridPeriod.Scope,
            StartDate = GridPeriod.StartDate,
            Length = GridPeriod.Scope == BudgetScope.Monthly ? DateConstants.MONTHS_IN_YEAR : 1
        };

        var category = Categories.First(c => c.Id == EditModel.CategoryId);

        var entry = new BudgetEntry
        {
            Id = EditModel.Id ?? Guid.NewGuid(),
            CategoryId = category.Id,
            Category = new Category
            {
                Id = category.Id,
                Name = category.Name,
                GroupId = category.GroupId,
                Group = new CategoryGroup
                {
                    Id = category.GroupId,
                    Name = category.GroupName,
                    IsIncome = category.IsIncome
                }
            },
            Amount = EditModel.Amount,
            Notes = EditModel.Notes,
            PeriodId = period.Id,
            Period = period,
            PeriodPosition = GridPeriod.Scope switch
            {
                BudgetScope.Monthly => EditModel.StartCellIndex,
                BudgetScope.Weekly => 0,
                BudgetScope.Fortnightly => 0,
                _ => 0
            }
        };

        try
        {
            if (IsEditing)
            {
                await Workflow.SaveAsync(entry, isEditing: true);
                Validation.SetSuccess("Budget entry updated");
            }
            else
            {
                await Workflow.SaveAsync(entry, isEditing: false);
                Validation.SetSuccess("Budget entry created");
            }

            await CloseEditModal();
            await ReloadAsync();
        }
        catch (Exception ex)
        {
            Validation.SetError(ex.Message);
        }
    }

    public async Task Delete()
    {
        if (EditModel?.Id is null) return;

        try
        {
            await Workflow.DeleteAsync(EditModel.Id.Value);
            Validation.SetSuccess("Budget entry deleted");
            await CloseEditModal();
            await ReloadAsync();
        }
        catch (Exception ex)
        {
            Validation.SetError(ex.Message);
        }
    }
}
