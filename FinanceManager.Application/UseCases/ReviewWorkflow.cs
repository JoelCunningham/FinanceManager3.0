namespace FinanceManager.Application.UseCases;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.UseCases.Categories;
using FinanceManager.Application.UseCases.Review;

public sealed class ReviewWorkflow(GetReviewPage getReviewPage, GetTransferCandidates getTransferCandidates, GetReimbursementCandidates getReimbursementCandidates, SaveReview saveReview, SuggestCategory suggestCategory, GetCategoryList getCategoryList)
{
    public Task<GetReviewPageResult> GetPageAsync(FilterQuery query) => getReviewPage.ExecuteAsync(query);
    public Task<GetTransferCandidatesResult> GetTransferCandidatesAsync(FilterQuery query, decimal amount) => getTransferCandidates.ExecuteAsync(query, amount);
    public Task<GetReimbursementCandidatesResult> GetReimbursementCandidatesAsync(FilterQuery query) => getReimbursementCandidates.ExecuteAsync(query);
    public Task<SaveReviewResult> SaveAsync(ReviewGroup group) => saveReview.ExecuteAsync(group);
    public Task<SuggestCategoryResult> SuggestCategoryAsync(string description) => suggestCategory.ExecuteAsync(description);
    public Task<GetCategoryListResult> GetCategoriesAsync() => getCategoryList.ExecuteAsync();
}
