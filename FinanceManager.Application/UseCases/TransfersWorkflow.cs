namespace FinanceManager.Application.UseCases;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.UseCases.Transfers;

public sealed class TransfersWorkflow(GetUniqueAccounts getUniqueTransferAccounts, GetTransfersPage getTransfersPage, SeparateTransfer separateTransfer)
{
    public Task<GetUniqueAccountsResult> GetAccountsAsync() => getUniqueTransferAccounts.ExecuteAsync();
    public Task<GetTransfersPageResult> GetPageAsync(FilterQuery query) => getTransfersPage.ExecuteAsync(query);
    public Task<SeparateTransferResult> SeparateAsync(Guid transferId) => separateTransfer.ExecuteAsync(transferId);
}
