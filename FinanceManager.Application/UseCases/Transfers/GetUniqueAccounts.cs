namespace FinanceManager.Application.UseCases.Transfers;

using FinanceManager.Application.Interfaces;

public sealed record GetUniqueAccountsResult(IReadOnlyList<string> Accounts) : UseCaseResult;

public sealed class GetUniqueAccounts(ITransferRepository transferRepository)
{
    public async Task<GetUniqueAccountsResult> ExecuteAsync()
    {
        var accounts = await transferRepository.GetUniqueAccountsAsync();
        return new GetUniqueAccountsResult(accounts);
    }
}
