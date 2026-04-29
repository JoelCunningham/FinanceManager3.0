namespace FinanceManager.WebApp.Components.Pages;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Enums;
using FinanceManager.Application.UseCases;
using FinanceManager.Application.UseCases.Transfers;
using FinanceManager.WebApp.Components.Features.Transactions;
using FinanceManager.WebApp.Components.Features.Transfers;
using FinanceManager.WebApp.Models;
using Havit.Blazor.Components.Web;
using Microsoft.AspNetCore.Components;

public partial class Transactions : ComponentBase
{
    [Inject] public TransactionsWorkflow TransactionsWorkflow { get; set; } = default!;
    [Inject] public TransfersWorkflow TransfersWorkflow { get; set; } = default!;

    [Inject] public IHxMessengerService Messenger { get; set; } = default!;
    public ValidationModel Validation { get; set; } = new();

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

    protected override async Task OnInitializedAsync()
    {
        Validation.Messenger = Messenger;

        UniqueAccounts = (await TransactionsWorkflow.GetUniqueAccountsAsync()).Accounts;
        Categories = (await TransactionsWorkflow.GetCategoriesAsync()).Categories;

        TransactionData.GetDataFunc = async (query) => (await TransactionsWorkflow.GetPagedTransactionsAsync(query)).Page;
        TransferData.GetDataFunc = async (query) => (await TransfersWorkflow.GetPageAsync(query)).Page;
    }

    public async Task OpenTransactionDetailsModal(TransactionSummary transaction)
    {
        SelectedDetails = (await TransactionsWorkflow.GetDetailsAsync(transaction.TransactionId)).Transaction;
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

        var validationResult = await TransactionsWorkflow.ValidateTransactionEditAsync(EditTransaction);
        if (!validationResult.IsSuccess)
        {
            Validation.SetErrors(validationResult.Errors);
            return;
        }

        var saveResult = await TransactionsWorkflow.SaveTransactionEditAsync(EditTransaction);
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
        var result = await TransfersWorkflow.SeparateAsync(transfer.Id);
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
