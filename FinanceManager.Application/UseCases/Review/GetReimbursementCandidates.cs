namespace FinanceManager.Application.UseCases.Review;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Enums;
using FinanceManager.Application.Interfaces;

public sealed record GetReimbursementCandidatesResult(PagedResult<TransactionSummary> Page) : UseCaseResult;

public sealed class GetReimbursementCandidates(ITransactionRepository transactionRepository)
{
    public async Task<GetReimbursementCandidatesResult> ExecuteAsync(FilterQuery query)
    {
        query.FilterStatus = ReviewStatus.Reviewed;
        query.FilterAmountMax = 0;

        var paged = await transactionRepository.GetPagedTransactionsAsync(query);
        return new GetReimbursementCandidatesResult(
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
