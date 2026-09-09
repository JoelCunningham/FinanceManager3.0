namespace FinanceManager.WebApp.Components.Pages.Features.Transactions;

using FinanceManager.Application.Constants.Navigation;
using FinanceManager.Application.DTOs;
using FinanceManager.Application.Enums;
using FinanceManager.Application.UseCases.Budget;
using FinanceManager.Application.UseCases.Categories;
using FinanceManager.Application.UseCases.Review;
using FinanceManager.Application.UseCases.Transactions;
using FinanceManager.Application.UseCases.Transfers;
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
    [Inject] protected GetUniqueAccounts GetUniqueAccountsUseCase { get; set; } = default!;
    [Inject] protected GetCategories GetCategoriesUseCase { get; set; } = default!;
    [Inject] protected GetBudgetYears GetBudgetYearsUseCase { get; set; } = default!;
    [Inject] protected GetPagedTransactions GetPagedTransactionsUseCase { get; set; } = default!;
    [Inject] protected GetPagedTransfers GetPagedTransfersUseCase { get; set; } = default!;
    [Inject] protected GetTransactionDetails GetTransactionDetailsUseCase { get; set; } = default!;
    [Inject] protected GetPagedReview GetPagedReviewUseCase { get; set; } = default!;
    [Inject] protected GetReviewGroup GetReviewGroupUseCase { get; set; } = default!;
    [Inject] protected ValidateTransactionEdit ValidateTransactionEditUseCase { get; set; } = default!;
    [Inject] protected SaveTransactionEdit SaveTransactionEditUseCase { get; set; } = default!;
    [Inject] protected SaveReview SaveReviewUseCase { get; set; } = default!;
    [Inject] protected SeparateTransfer SeparateTransferUseCase { get; set; } = default!;
    [Inject] protected GetPagedBudget GetPagedBudgetUseCase { get; set; } = default!;
    [Inject] protected ExportTransactions ExportTransactionsUseCase { get; set; } = default!;

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
        if (!RendererInfo.IsInteractive) return;

        await base.OnInitializedAsync();

        UniqueAccounts = (await GetUniqueAccountsUseCase.ExecuteAsync()).Accounts;
        Categories = (await GetCategoriesUseCase.ExecuteAsync()).Categories;

        AvailableYears = (await GetBudgetYearsUseCase.ExecuteAsync()).AvailableYears.OrderByDescending(y => y);
        await ReloadSummary(DateTime.Now.Year);

        TransactionData.Query.FilterStatus = ReviewStatus.Reviewed;

        TransactionData.GetDataFunc = async (query) => (await GetPagedTransactionsUseCase.ExecuteAsync(query)).Page;
        TransactionData.UpdateViewState = StateHasChanged;

        TransferData.GetDataFunc = async (query) => (await GetPagedTransfersUseCase.ExecuteAsync(query)).Page;
        TransferData.UpdateViewState = StateHasChanged;

        UnreviewedCount = (await GetPagedReviewUseCase.ExecuteAsync(new FilterQuery { FilterStatus = ReviewStatus.Unreviewed })).Page.TotalItems;
        HideEmptyCategories = HasPopulatedCategories && await Preferences.HideEmptyActivityCategories;
    }

    public async Task OpenTransactionDetailsModal(TransactionSummary transaction)
    {
        SelectedDetails = (await GetTransactionDetailsUseCase.ExecuteAsync(transaction.EntityId)).Transaction;
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
        EditTransactionGroup = (await GetReviewGroupUseCase.ExecuteAsync(transaction.EntityId)).Group;
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

    public async Task SetDate(TransactionSummary transaction, DateTime value, DateTime recordDate)
    {
        Validation.ClearValidationItem(transaction.EntityId, ValidationField.Date);
        var result = await BackdateTransaction.ExecuteAsync(transaction, value, recordDate);

        if (!result.IsSuccess) Validation.SetErrors(result.Errors);
    }

    public async Task SetAmount(ReviewTransaction transaction, decimal value, ReviewGroup group)
    {
        Validation.ClearValidationItem(transaction.EntityId, ValidationField.Amount);
        var result = await UpdateTransactionAmount.ExecuteAsync(transaction, value, group);

        if (!result.IsSuccess) Validation.SetErrors(result.Errors);
    }

    public async Task SaveEdit()
    {
        Validation.Clear();

        var validationResult = await ValidateTransactionEditUseCase.ExecuteAsync(EditTransaction);
        if (!validationResult.IsSuccess)
        {
            Validation.SetErrors(validationResult.Errors);
            return;
        }

        var saveResult = await SaveTransactionEditUseCase.ExecuteAsync(EditTransaction);
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

        var validationResult = await ValidateReviewGroup.ExecuteAsync(EditTransactionGroup);
        if (!validationResult.IsSuccess)
        {
            Validation.SetErrors(validationResult.Errors);
            return;
        }

        var saveResult = await SaveReviewUseCase.ExecuteAsync(EditTransactionGroup);
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
        var result = await SeparateTransferUseCase.ExecuteAsync(transfer.Id);
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
        var pagedBudget = await GetPagedBudgetUseCase.ExecuteAsync(year, EntryTypeFilter);
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
        var result = await ExportTransactionsUseCase.ExecuteAsync(CurrentExportType);

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
