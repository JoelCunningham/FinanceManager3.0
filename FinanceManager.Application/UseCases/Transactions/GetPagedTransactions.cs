namespace FinanceManager.Application.UseCases.Transactions;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Interfaces;

public sealed record GetPagedTransactionsResult(PagedResult<TransactionSummary> Page) : UseCaseResult;

public sealed class GetPagedTransactions(ITransactionRepository transactionRepository)
{
    public async Task<GetPagedTransactionsResult> ExecuteAsync(FilterQuery query)
    {
        var pagedTransactions = await transactionRepository.GetPagedTransactionsAsync(query);

        return new GetPagedTransactionsResult
        (
            new PagedResult<TransactionSummary>
            {
                Items = [.. pagedTransactions.Items.Select(TransactionSummary.FromTransactions)],
                TotalItems = pagedTransactions.TotalItems,
                CurrentPage = pagedTransactions.CurrentPage,
                PageSize = pagedTransactions.PageSize
            }
        );
    }
}