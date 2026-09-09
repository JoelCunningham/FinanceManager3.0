namespace FinanceManager.Application.UseCases.Dashboard;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Enums;
using FinanceManager.Application.Interfaces;
using FinanceManager.Application.UseCases;

public sealed record GetUserStatusResult(UserStatus Status) : UseCaseResult;

public sealed class GetUserStatus(ITransactionRepository transactionRepository, IBudgetEntryRepository budgetEntryRepository)
{
    public async Task<GetUserStatusResult> ExecuteAsync(DateOnly? staleCutoff = null)
    {
        var transactions = await transactionRepository.GetTransactionsAsync(new FilterQuery());
        var budgetEntries = await budgetEntryRepository.GetAllAsync();

        var staleTransactions = transactions.Where(t => DateOnly.FromDateTime(t.Date) <= (staleCutoff ?? DefaultStaleCutoff));
        var staleBudgetEntries = budgetEntries.Where(b => b.EndDate <= (staleCutoff ?? DefaultStaleCutoff));

        if (!transactions.Any() && !budgetEntries.Any())
        {
            return new GetUserStatusResult(UserStatus.New);
        }
        if (!staleTransactions.Any() && !staleBudgetEntries.Any())
        {
            return new GetUserStatusResult(UserStatus.Stale);
        }
        return new GetUserStatusResult(UserStatus.Active);
    }

    private readonly DateOnly DefaultStaleCutoff = DateOnly.FromDateTime(DateTime.Now.AddYears(-1));
}