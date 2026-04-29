namespace FinanceManager.Application.UseCases.Transactions;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Interfaces;

public sealed record GetTransactionsPageResult(PagedResult<TransactionSummary> Page) : UseCaseResult;

public sealed class GetPagedTransactions(ITransactionRepository transactionRepository)
{
    public async Task<GetTransactionsPageResult> ExecuteAsync(FilterQuery query)
    {
        var pagedTransactions = await transactionRepository.GetPagedAsync(query);

        return new GetTransactionsPageResult
        (
            new PagedResult<TransactionSummary>
            {
                Items = [.. pagedTransactions.Items.Select(TransactionSummary.FromTransaction)],
                TotalItems = pagedTransactions.TotalItems,
                CurrentPage = pagedTransactions.CurrentPage,
                PageSize = pagedTransactions.PageSize
            }
        );
    }
}