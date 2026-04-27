namespace FinanceManager.Application.UseCases.Review;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Interfaces;

public sealed record GetReviewPageResult(PagedResult<ReviewGroup> Page) : UseCaseResult();

public sealed class GetReviewPage(ITransactionRepository transactionRepository)
{
    public async Task<GetReviewPageResult> ExecuteAsync(FilterQuery query)
    {
        var paged = await transactionRepository.GetPagedAsync(query);

        return new GetReviewPageResult
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
