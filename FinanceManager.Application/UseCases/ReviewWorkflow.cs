namespace FinanceManager.Application.UseCases;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.UseCases.Categories;
using FinanceManager.Application.UseCases.Review;
using FinanceManager.Domain.Entities;

public sealed class ReviewWorkflow(GetReviewPage getReviewPage, GetTransferCandidates getTransferCandidates, GetReimbursementCandidates getReimbursementCandidates, SaveReview saveReview, GetCategoryList getCategoryList, AutoAssignCategories autoAssignCategories)
{
    public Task<GetReviewPageResult> GetPageAsync(FilterQuery query) => getReviewPage.ExecuteAsync(query);
    public Task<GetTransferCandidatesResult> GetTransferCandidatesAsync(FilterQuery query, decimal amount) => getTransferCandidates.ExecuteAsync(query, amount);
    public Task<GetReimbursementCandidatesResult> GetReimbursementCandidatesAsync(FilterQuery query) => getReimbursementCandidates.ExecuteAsync(query);
    public Task<SaveReviewResult> SaveAsync(ReviewGroup group) => saveReview.ExecuteAsync(group);
    public Task<AutoCategoriseResult> AutoCategoriseAsync(IEnumerable<ReviewGroup> groups, IEnumerable<Category> categories) => autoAssignCategories.ExecuteAsync(groups, categories);
    public Task<GetCategoryListResult> GetCategoriesAsync() => getCategoryList.ExecuteAsync();
    public static Task<ValidateReviewGroupResult> ValidateGroupAsync(ReviewGroup group) => ValidateReviewGroup.ExecuteAsync(group);
}
