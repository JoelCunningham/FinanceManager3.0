namespace FinanceManager.Application.UseCases.Review;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Interfaces;

public sealed record GetTransferCandidatesResult(PagedResult<TransactionSummary> Page) : UseCaseResult;

public sealed class GetTransferCandidates(ITransactionRepository transactionRepository)
{
    public async Task<GetTransferCandidatesResult> ExecuteAsync(FilterQuery query, decimal amount)
    {
        query.FilterAmountMax = -amount;
        query.FilterAmountMin = -amount;

        var paged = await transactionRepository.GetPagedTransactionsAsync(query);
        return new GetTransferCandidatesResult(
            new PagedResult<TransactionSummary>
            {
                Items = [.. paged.Items.Select(TransactionSummary.FromTransaction)],
                TotalItems = paged.TotalItems,
                CurrentPage = paged.CurrentPage,
                PageSize = paged.PageSize
            }
        );
    }
}
