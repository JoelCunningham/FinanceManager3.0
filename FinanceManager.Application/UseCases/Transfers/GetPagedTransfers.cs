namespace FinanceManager.Application.UseCases.Transfers;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Entities;

public sealed record GetPagedTransfersResult(PagedResult<TransferDto> Page) : UseCaseResult;

public sealed class GetPagedTransfers(ITransferRepository transferRepository)
{
    public async Task<GetPagedTransfersResult> ExecuteAsync(FilterQuery query)
    {
        var pagedTransfers = await transferRepository.GetPagedAsync(query);

        return new GetPagedTransfersResult
        (
           new PagedResult<TransferDto>
           {
               Items = [.. pagedTransfers.Items.Select(FromTransfer)],
               TotalItems = pagedTransfers.TotalItems,
               CurrentPage = pagedTransfers.CurrentPage,
               PageSize = pagedTransfers.PageSize
           }
        );
    }

    private static TransferDto FromTransfer(Transfer transfer)
    {
        return new TransferDto(
            transfer.Id, transfer.Amount,
            new Transferable(transfer.FromRecord.BankAccount.Bank, transfer.FromRecord.BankAccount.AccountNumber),
            new Transferable(transfer.ToRecord.BankAccount.Bank, transfer.ToRecord.BankAccount.AccountNumber),
            transfer.Date, transfer.Description, transfer.IsUserCreated
        );
    }
}

public sealed record TransferDto(
    Guid Id,
    decimal Amount,
    Transferable From,
    Transferable To,
    DateTime Date,
    string Description,
    bool IsUserCreated
);

public sealed record Transferable(
    string Bank,
    string? Account
);
