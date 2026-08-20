namespace FinanceManager.WebApp.Components.Pages.Features.Transactions;

using FinanceManager.Application.Constants.Navigation;
using FinanceManager.Application.DTOs;
using FinanceManager.Application.Enums;
using FinanceManager.Application.UseCases;
using FinanceManager.Domain.Enums;
using FinanceManager.WebApp.Components.Base;
using FinanceManager.WebApp.Components.Shared.Budget;
using FinanceManager.WebApp.Models;
using FinanceManager.WebApp.Utilities;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

[Route(Pages.Activities)]
[Route(Pages.Activities + Tabs.ActiveTabId)]
public partial class _Page : MainPageBase
{
    public DataGridModel<FilterQuery, TransactionSummary> TransactionData { get; set; } = new(16);
    public DataGridModel<FilterQuery, TransferSummary> TransferData { get; set; } = new(16);

    public IReadOnlyList<string> UniqueAccounts { get; set; } = [];
    public IReadOnlyList<CategorySummary> Categories { get; set; } = [];
    private IReadOnlyList<BudgetColumn> SummaryColumns { get; set; } = [];

    public TransactionDetails SelectedDetails { get; set; } = default!;
    public TransactionSummary EditTransaction { get; set; } = default!;
    public ReviewGroup EditTransactionGroup { get; set; } = default!;
    public TransferSummary? SelectedTransfer { get; set; }

    public TransactionDetailsModal DetailsModal { get; set; } = new();
    public EditTransactionModal EditModal { get; set; } = new();
    public TransferModal TransferModal { get; set; } = new();
    public BudgetCellModal BudgetCellModal { get; set; } = new();
    public ExportModal ExportModal { get; set; } = new();

    private bool HideEmptyCategories { get; set; }
    private BudgetGridMode EntryTypeFilter { get; set; } = BudgetGridMode.Net;

    private BudgetYearSummary? ActiveBudgetYear { get; set; }
    private IEnumerable<int> AvailableYears { get; set; } = [];

    private BudgetCell CurrentCell { get; set; } = default!;
    private BudgetColumn CurrentColumn { get; set; } = default!;
    private ExportType CurrentExportType { get; set; } = ExportType.BankRecords;

    private int UnreviewedCount { get; set; } = 0;
    private bool HasPopulatedCategories => SummaryColumns.Any(c => c.Cells.Count != 0);

    private static readonly BudgetGridMode[] EntryTypeFilters =
    [
        BudgetGridMode.Net,
        BudgetGridMode.Income,
        BudgetGridMode.Expense
    ];

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        UniqueAccounts = (await UseCases.GetUniqueAccountsAsync()).Accounts;
        Categories = (await UseCases.GetCategoriesAsync()).Categories;

        AvailableYears = (await UseCases.GetBudgetYearsAsync()).AvailableYears.OrderByDescending(y => y);
        await ReloadSummary(DateTime.Now.Year);

        TransactionData.Query.FilterStatus = ReviewStatus.Reviewed;

        TransactionData.GetDataFunc = async (query) => (await UseCases.GetPagedTransactionsAsync(query)).Page;
        TransactionData.UpdateViewState = StateHasChanged;

        TransferData.GetDataFunc = async (query) => (await UseCases.GetPagedTransfersAsync(query)).Page;
        TransferData.UpdateViewState = StateHasChanged;

        UnreviewedCount = (await UseCases.GetPagedReviewAsync(new FilterQuery { FilterStatus = ReviewStatus.Unreviewed })).Page.TotalItems;
        HideEmptyCategories = HasPopulatedCategories && await Preferences.HideEmptyActivityCategories;
    }

    public async Task OpenTransactionDetailsModal(TransactionSummary transaction)
    {
        SelectedDetails = (await UseCases.GetTransactionDetailsAsync(transaction.EntityId)).Transaction;
        await DetailsModal.ShowAsync();
    }

    public async Task OpenTransferDetailsModal(TransferSummary transfer)
    {
        SelectedTransfer = transfer;
        await TransferModal.ShowAsync();
    }

    public async Task OpenEditModal(TransactionSummary transaction)
    {
        Validation.Clear();
        EditTransaction = transaction.Clone();
        EditTransactionGroup = (await UseCases.GetReviewGroupAsync(transaction.EntityId)).Group;
        await EditModal.ShowAsync();
    }

    public void SetCategory(CategorySummary? category)
    {
        Validation.ClearValidationItem(EditTransaction.EntityId, ValidationField.Category);
        EditTransaction.Category = category;
    }

    public void SetCategory(ReviewTransaction transaction, CategorySummary? category)
    {
        Validation.ClearValidationItem(transaction.EntityId, ValidationField.Category);
        transaction.Category = category;
    }

    public void SetDate(TransactionSummary transaction, DateTime value, DateTime recordDate)
    {
        Validation.ClearValidationItem(transaction.EntityId, ValidationField.Date);
        var result = UseCases.BackdateTransactionAsync(transaction, value, recordDate).Result;

        if (!result.IsSuccess) Validation.SetErrors(result.Errors);
    }

    public async Task SetAmount(ReviewTransaction transaction, decimal value, ReviewGroup group)
    {
        Validation.ClearValidationItem(transaction.EntityId, ValidationField.Amount);
        var result = await UseCases.UpdateTransactionAmountAsync(transaction, value, group);

        if (!result.IsSuccess) Validation.SetErrors(result.Errors);
    }

    public async Task SaveEdit()
    {
        Validation.Clear();

        var validationResult = await UseCases.ValidateTransactionEditAsync(EditTransaction);
        if (!validationResult.IsSuccess)
        {
            Validation.SetErrors(validationResult.Errors);
            return;
        }

        var saveResult = await UseCases.SaveTransactionEditAsync(EditTransaction);
        if (!saveResult.IsSuccess)
        {
            Validation.SetErrors(saveResult.Errors);
            return;
        }

        Validation.SetSuccess("Activity updated successfully.");
        await TransactionData.UpdateAsync();
        await EditModal.HideAsync();
    }

    public async Task SaveGroupEdit()
    {
        Validation.Clear();

        var validationResult = await UseCases.ValidateReviewGroupAsync(EditTransactionGroup);
        if (!validationResult.IsSuccess)
        {
            Validation.SetErrors(validationResult.Errors);
            return;
        }

        var saveResult = await UseCases.SaveReviewAsync(EditTransactionGroup);
        if (!saveResult.IsSuccess)
        {
            Validation.SetErrors(saveResult.Errors);
            return;
        }

        Validation.SetSuccess("Activity updated successfully.");
        await TransactionData.UpdateAsync();
        await EditModal.HideAsync();
    }

    private async Task SeparateTransfer(TransferSummary transfer)
    {
        var result = await UseCases.SeparateTransferAsync(transfer.Id);
        if (result.IsSuccess)
        {
            Validation.SetSuccess("Transfer separated successfully.");
            await TransferData.UpdateAsync();
        }
        else
        {
            Validation.SetError("Could not remove transfer. Please try again.");
        }
    }

    private async Task SeparateFromDetails()
    {
        if (SelectedTransfer != null)
        {
            await SeparateTransfer(SelectedTransfer);
            await TransferModal.HideAsync();
        }
    }

    private async Task OnHideEmptyCategoriesChanged(bool value)
    {
        HideEmptyCategories = value;
        await Preferences.Set(PreferenceNames.HideEmptyActivityCategories, value);
    }

    private async Task OnEntryTypeFilterChanged(BudgetGridMode mode)
    {
        EntryTypeFilter = mode;
        await ReloadSummary(DateTime.Now.Year);
    }

    private async Task OnRangeSelected(int index)
    {
        await ReloadSummary(AvailableYears.ElementAtOrDefault(index));
    }

    private async Task ReloadSummary(int year)
    {
        var pagedBudget = await UseCases.GetPagedBudgetAsync(year, EntryTypeFilter);
        SummaryColumns = pagedBudget.Columns;
        ActiveBudgetYear = pagedBudget.BudgetYear;
    }

    private async Task OpenCellModal(BudgetCell cell, BudgetColumn column)
    {
        CurrentColumn = column;
        CurrentCell = cell;
        await BudgetCellModal.ShowAsync();
    }

    private async Task CloseCellModal()
    {
        await BudgetCellModal.HideAsync();
    }

    private async Task OpenExportModal()
    {
        await ExportModal.ShowAsync();
    }

    private async Task ExportTransactions()
    {
        var result = await UseCases.ExportTransactionsAsync(CurrentExportType);

        if (!result.IsSuccess)
        {
            Validation.SetError("Could not export transactions. Please try again.");
        }
        else
        {
            await JS.InvokeVoidAsync(JsCommands.DownloadFile, result.FileName, result.ContentType, result.Content);

            await ExportModal.HideAsync();
            Validation.SetSuccess("Exported started successfully.");
        }
    }
}
