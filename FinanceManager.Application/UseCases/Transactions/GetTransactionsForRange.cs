namespace FinanceManager.Application.UseCases.Transactions;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Interfaces;

public sealed record GetTransactionsForRangeResult(IReadOnlyList<TransactionSummary> Transactions) : UseCaseResult;

public sealed class GetTransactionsForRange(ITransactionRepository transactionRepository)
{
    public async Task<GetTransactionsForRangeResult> ExecuteAsync(FilterQuery query)
    {
        var results = new List<TransactionSummary>();
        var page = 1;

        while (true)
        {
            var pagedTransactions = await transactionRepository.GetPagedAsync(query);
            var pageResult = new PagedResult<TransactionSummary>
            {
                Items = [.. pagedTransactions.Items.Select(TransactionSummary.FromTransaction)],
                TotalItems = pagedTransactions.TotalItems,
                CurrentPage = pagedTransactions.CurrentPage,
                PageSize = pagedTransactions.PageSize
            };

            if (pageResult.Items.Count == 0) break;
            results.AddRange(pageResult.Items);

            if (results.Count >= pageResult.TotalItems) break;
            page++;
        }

        return new GetTransactionsForRangeResult(results);
    }
}