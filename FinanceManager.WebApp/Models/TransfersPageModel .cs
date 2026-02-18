namespace FinanceManager.WebApp.Models;

using FinanceManager.Application.DTOs;
using FinanceManager.WebApp.Components.Features.Transfers;
using FinanceManager.WebApp.Models.Base;
using Havit.Blazor.Components.Web.Bootstrap;

public class TransfersPageModel
{
    public ValidationModel Validation { get; set; } = new();

    public FilterQuery Query { get; set; } = new();
    public List<string> UniqueAccounts { get; set; } = [];
    public TransferSummary? SelectedTransfer { get; set; }

    public TransfersGrid? Grid { get; set; }
    public HxModal DetailsModal { get; set; } = new();

    public async Task OpenDetailsModal(TransferSummary transfer)
    {
        SelectedTransfer = transfer;
        await DetailsModal.ShowAsync();
    }
}