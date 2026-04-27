namespace FinanceManager.WebApp.Components.Pages;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Enums;
using FinanceManager.Application.UseCases;
using FinanceManager.WebApp.Components.Features.Transactions;
using FinanceManager.WebApp.Models;
using Havit.Blazor.Components.Web;
using Microsoft.AspNetCore.Components;

public partial class Transactions : ComponentBase
{
    [Inject] public TransactionsWorkflow Workflow { get; set; } = default!;
    [Inject] public IHxMessengerService Messenger { get; set; } = default!;

    public DataGridModel<FilterQuery, TransactionSummary> Data { get; set; } = new(20);
    public ValidationModel Validation { get; set; } = new();

    public IReadOnlyList<string> UniqueAccounts { get; set; } = [];
    public IReadOnlyList<CategorySummary> Categories { get; set; } = [];

    public TransactionDetails SelectedDetails { get; set; } = default!;
    public TransactionSummary EditTransaction { get; set; } = default!;

    public TransactionDetailsModal DetailsModal { get; set; } = new();
    public EditTransactionModal EditModal { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        Validation.Messenger = Messenger;

        UniqueAccounts = (await Workflow.GetUniqueAccountsAsync()).Accounts;
        Categories = (await Workflow.GetCategoriesAsync()).Categories;

        Data.GetDataFunc = async (query) => (await Workflow.GetPagedTransactionsAsync(query)).Page;
    }

    public async Task OpenDetailsModal(TransactionSummary transaction)
    {
        SelectedDetails = (await Workflow.GetDetailsAsync(transaction.TransactionId)).Transaction;
        await DetailsModal.ShowAsync();
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

        var validationResult = await Workflow.ValidateTransactionEditAsync(EditTransaction);
        if (!validationResult.IsSuccess)
        {
            Validation.SetErrors(validationResult.Errors);
            return;
        }

        var saveResult = await Workflow.SaveTransactionEditAsync(EditTransaction);
        if (!saveResult.IsSuccess)
        {
            Validation.SetErrors(saveResult.Errors);
            return;
        }

        Validation.SetSuccess("Transaction updated successfully.");
        await Data.UpdateAsync();
        await EditModal.HideAsync();
    }
}
