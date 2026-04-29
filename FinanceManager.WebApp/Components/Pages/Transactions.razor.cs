namespace FinanceManager.WebApp.Components.Pages;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Enums;
using FinanceManager.Application.UseCases;
using FinanceManager.Application.UseCases.Transfers;
using FinanceManager.WebApp.Components.Features.Transactions;
using FinanceManager.WebApp.Components.Features.Transfers;
using FinanceManager.WebApp.Models;

public partial class Transactions : PageBase
{
    public DataGridModel<FilterQuery, TransactionSummary> TransactionData { get; set; } = new(20);
    public DataGridModel<FilterQuery, TransferDto> TransferData { get; set; } = new(20);

    public IReadOnlyList<string> UniqueAccounts { get; set; } = [];
    public IReadOnlyList<CategorySummary> Categories { get; set; } = [];

    public TransactionDetails SelectedDetails { get; set; } = default!;
    public TransactionSummary EditTransaction { get; set; } = default!;
    public TransferDto? SelectedTransfer { get; set; }

    public TransactionDetailsModal DetailsModal { get; set; } = new();
    public EditTransactionModal EditModal { get; set; } = new();
    public TransferModal TransferModal { get; set; } = new();

    public int UnreviewedCount = 0;

    protected override async Task OnInitializedAsync()
    {
        Validation.Messenger = Messenger;

        UniqueAccounts = (await UseCases.GetUniqueAccountsAsync()).Accounts;
        Categories = (await UseCases.GetCategoryListAsync()).Categories;

        TransactionData.Query.FilterStatus = ReviewStatus.Reviewed;
        TransactionData.GetDataFunc = async (query) => (await UseCases.GetPagedTransactionsAsync(query)).Page;
        TransferData.GetDataFunc = async (query) => (await UseCases.GetPagedTransfersAsync(query)).Page;

        UnreviewedCount = (await UseCases.GetPagedReviewAsync(new FilterQuery { FilterStatus = ReviewStatus.Unreviewed })).Page.TotalItems;
    }

    public async Task OpenTransactionDetailsModal(TransactionSummary transaction)
    {
        SelectedDetails = (await UseCases.GetTransactionDetailsAsync(transaction.TransactionId)).Transaction;
        await DetailsModal.ShowAsync();
    }

    public async Task OpenTransferDetailsModal(TransferDto transfer)
    {
        SelectedTransfer = transfer;
        await TransferModal.ShowAsync();
    }

    public async Task OpenEditModal(TransactionSummary transaction)
    {
        Validation.Clear();
        EditTransaction = transaction.Clone();
        await EditModal.ShowAsync();
    }

    public void SetEditCategory(CategorySummary? category)
    {
        Validation.ClearValidationItem(EditTransaction.TransactionId, ValidationField.Category);
        EditTransaction.Category = category;
    }

    public void SetEditDate(DateTime date)
    {
        Validation.ClearValidationItem(EditTransaction.TransactionId, ValidationField.Date);
        EditTransaction.Date = date;
    }

    public void SetEditAmount(decimal amount)
    {
        Validation.ClearValidationItem(EditTransaction.TransactionId, ValidationField.Amount);
        EditTransaction.Amount = amount;
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

        Validation.SetSuccess("Transaction updated successfully.");
        await TransactionData.UpdateAsync();
        await EditModal.HideAsync();
    }

    private async Task SeparateTransfer(TransferDto transfer)
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
}
