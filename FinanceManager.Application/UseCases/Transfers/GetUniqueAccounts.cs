namespace FinanceManager.Application.UseCases.Transfers;

using FinanceManager.Application.Interfaces;

public sealed record GetUniqueAccountsResult(IReadOnlyList<string> Accounts) : UseCaseResult;

public sealed class GetUniqueAccounts(IBankAccountRepository bankAccountRepository)
{
    public async Task<GetUniqueAccountsResult> ExecuteAsync()
    {
        var accounts = await bankAccountRepository.GetAllAsync();
        return new GetUniqueAccountsResult([.. accounts.Select(a => a.DisplayName)]);
    }
}
