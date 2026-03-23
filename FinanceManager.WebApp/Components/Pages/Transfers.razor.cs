namespace FinanceManager.WebApp.Components.Pages;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.UseCases;
using FinanceManager.Application.UseCases.Transfers;
using FinanceManager.WebApp.Components.Features.Transfers;
using FinanceManager.WebApp.Models;
using Microsoft.AspNetCore.Components;

public partial class Transfers : ComponentBase
{
    [Inject] public TransfersWorkflow Workflow { get; set; } = default!;

    public DataGridModel<FilterQuery, TransferDto> Data { get; set; } = new(20);
    public ValidationModel Validation { get; set; } = new();

    public IReadOnlyList<string> UniqueAccounts { get; set; } = [];
    public TransferDto? SelectedTransfer { get; set; }
    public TransferModal TransferModal { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        UniqueAccounts = (await Workflow.GetAccountsAsync()).Accounts;
        Data.GetDataFunc = async (query) => (await Workflow.GetPageAsync(query)).Page;
    }

    private async Task SeparateTransfer(TransferDto transfer)
    {
        var result = await Workflow.SeparateAsync(transfer.Id);
        if (result.IsSuccess)
        {
            Validation.SetSuccess("Transfer separated successfully.");
            await Data.UpdateAsync();
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

    public async Task OpenDetailsModal(TransferDto transfer)
    {
        SelectedTransfer = transfer;
        await TransferModal.ShowAsync();
    }
}