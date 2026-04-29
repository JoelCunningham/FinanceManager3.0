namespace FinanceManager.Application.UseCases.Review;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Enums;
using FinanceManager.Application.Interfaces;

public sealed record GetPagedReviewResult(PagedResult<ReviewGroup> Page) : UseCaseResult();

public sealed class GetPagedReview(ITransactionRepository transactionRepository)
{
    public async Task<GetPagedReviewResult> ExecuteAsync(FilterQuery query)
    {
        query.SortBy = TransactionSortBy.Date;
        query.FilterStatus = ReviewStatus.Unreviewed;

        var paged = await transactionRepository.GetPagedAsync(query);

        return new GetPagedReviewResult
        (
            new PagedResult<ReviewGroup>
            {
                Items = [.. paged.Items.Select(ReviewGroup.FromTransaction)],
                TotalItems = paged.TotalItems,
                CurrentPage = paged.CurrentPage,
                PageSize = paged.PageSize
            }
        );
    }
}
