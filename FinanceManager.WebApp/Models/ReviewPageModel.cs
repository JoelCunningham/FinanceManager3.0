using FinanceManager.Application.DTOs;
using FinanceManager.Domain.Entities;

namespace FinanceManager.WebApp.Models;

public class ReviewPageModel
{
    public PagedResult<ReviewGroup> PagedReviewGroups { get; set; } = new();
    public FilterQuery ReviewQuery { get; set; } = new() { FilterStatus = ReviewStatus.Unreviewed };

    public PagedResult<TransactionSummary> PagedSearchResults { get; set; } = new();
    public FilterQuery SearchQuery { get; set; } = new() { FilterStatus = ReviewStatus.Reviewed };

    public ReviewGroup? CurrentGroup { get; set; }
    public ReviewTransaction? CurrentTransaction { get; set; }

    public List<Category> Categories { get; set; } = [];
    public List<Category> IncomeCategories => [.. Categories.Where(c => c.Group.IsIncome)];
    public List<Category> ExpenseCategories => [.. Categories.Where(c => !c.Group.IsIncome)];

    public bool IsTransferSearchOpen { get; set; } = false;
    public bool IsReimbursementSearchOpen { get; set; } = false;

    public bool HasError { get; set; } = false;
    public string? ErrorMessage { get; set; }

    public void SetReviewPage(int page)
    {
        ReviewQuery = ReviewQuery with { Page = Math.Max(1, page) };
    }

    public void SetSearchPage(int page)
    {
        SearchQuery = SearchQuery with { Page = Math.Max(1, page) };
    }

    public void UpdateSearchFilters()
    {
        SearchQuery = SearchQuery with { Page = 1, FilterStatus = ReviewStatus.Reviewed };
    }

    public void ClearSearchFilters()
    {
        SearchQuery = new();
    }

    public void UpdateSearchFiltersForTransfer(decimal targetAmount)
    {
        SearchQuery = new FilterQuery { FilterStatus = ReviewStatus.All, FilterAmountMin = -targetAmount, FilterAmountMax = -targetAmount };
    }

    public void Clean()
    {
        CurrentGroup = null;
        CurrentTransaction = null;
        IsTransferSearchOpen = false;
        IsReimbursementSearchOpen = false;
        SearchQuery = new FilterQuery { FilterStatus = ReviewStatus.Reviewed };
    }

    public void SetError(string errorMessage)
    {
        HasError = true;
        ErrorMessage = ErrorMessage is null ? errorMessage : "Multiple problems detected";
    }
}