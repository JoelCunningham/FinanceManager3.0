namespace FinanceManager.WebApp.Components.Pages;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Services;
using FinanceManager.WebApp.Components.Features.Transfers;
using FinanceManager.WebApp.Models;
using Microsoft.AspNetCore.Components;

public partial class Transfers : ComponentBase
{
    [Inject] public TransferService TransferService { get; set; } = default!;

    public DataGridModel<FilterQuery, TransferSummary> Data { get; set; } = new(20);
    public ValidationModel Validation { get; set; } = new();

    public List<string> UniqueAccounts { get; set; } = [];
    public TransferSummary? SelectedTransfer { get; set; }
    public TransferModal TransferModal { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        UniqueAccounts = await TransferService.GetUniqueAccountsAsync();
        Data.GetDataFunc = async (query) => await TransferService.GetPagedAsync(query);
    }

    private async Task SeparateTransfer(TransferSummary transfer)
    {
        try
        {
            await TransferService.ConvertToTransactionAsync(transfer.Id);
            Validation.SetSuccess("Transfer separated successfully.");
            await Data.UpdateAsync();
        }
        catch (Exception)
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

    public async Task OpenDetailsModal(TransferSummary transfer)
    {
        SelectedTransfer = transfer;
        await TransferModal.ShowAsync();
    }
}