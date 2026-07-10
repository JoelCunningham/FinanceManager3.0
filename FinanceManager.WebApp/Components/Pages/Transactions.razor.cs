namespace FinanceManager.WebApp.Components.Pages;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Enums;
using FinanceManager.Application.UseCases;
using FinanceManager.Application.UseCases.Transfers;
using FinanceManager.WebApp.Components.Base;
using FinanceManager.WebApp.Components.Features.Transactions;
using FinanceManager.WebApp.Components.Features.Transfers;
using FinanceManager.WebApp.Models;
using FinanceManager.WebApp.Navigation;
using Microsoft.AspNetCore.Components;

[Route(Pages.Activities)]
[Route(Pages.Activities + Tabs.ActiveTabId)]
public partial class Transactions : PageBase
{
    public DataGridModel<FilterQuery, TransactionSummary> TransactionData { get; set; } = new(16);
    public DataGridModel<FilterQuery, TransferDto> TransferData { get; set; } = new(16);

    public IReadOnlyList<string> UniqueAccounts { get; set; } = [];
    public IReadOnlyList<CategorySummary> Categories { get; set; } = [];

    public TransactionDetails SelectedDetails { get; set; } = default!;
    public TransactionSummary EditTransaction { get; set; } = default!;
    public ReviewGroup EditTransactionGroup { get; set; } = default!;
    public TransferDto? SelectedTransfer { get; set; }

    public TransactionDetailsModal DetailsModal { get; set; } = new();
    public EditTransactionModal EditModal { get; set; } = new();
    public TransferModal TransferModal { get; set; } = new();

    public int UnreviewedCount = 0;

    protected override async Task OnInitializedAsync()
    {
        Validation.Messenger = Messenger;

        UniqueAccounts = (await UseCases.GetUniqueAccountsAsync()).Accounts;
        Categories = (await UseCases.GetCategoriesAsync()).Categories;

        TransactionData.Query.FilterStatus = ReviewStatus.Reviewed;

        TransactionData.GetDataFunc = async (query) => (await UseCases.GetPagedTransactionsAsync(query)).Page;
        TransactionData.UpdateViewState = StateHasChanged;

        TransferData.GetDataFunc = async (query) => (await UseCases.GetPagedTransfersAsync(query)).Page;
        TransferData.UpdateViewState = StateHasChanged;

        UnreviewedCount = (await UseCases.GetPagedReviewAsync(new FilterQuery { FilterStatus = ReviewStatus.Unreviewed })).Page.TotalItems;
    }

    public async Task OpenTransactionDetailsModal(TransactionSummary transaction)
    {
        SelectedDetails = (await UseCases.GetTransactionDetailsAsync(transaction.EntityId)).Transaction;
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
