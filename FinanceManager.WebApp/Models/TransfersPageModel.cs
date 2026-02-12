using FinanceManager.Application.DTOs;

namespace FinanceManager.WebApp.Models;

public class TransfersPageModel
{
    public PagedResult<TransferSummary> PagedTransfers { get; set; } = new();
    public List<string> UniqueAccounts { get; set; } = [];
    public FilterQuery Query { get; set; } = new();

    public TransferSummary? ErrorTransfer { get; set; }
    public string? ErrorMessage { get; set; }

    public void SetPage(int page)
    {
        Query = Query with { Page = Math.Max(1, page) };
    }

    public void UpdateFilters()
    {
        Query = Query with { Page = 1 };
    }

    public void ClearFilters()
    {
        Query = new();
    }

    public void SetError(TransferSummary transfer)
    {
        ErrorTransfer = transfer;
        ErrorMessage = "Could not remove transfer. Please try again.";
    }
}