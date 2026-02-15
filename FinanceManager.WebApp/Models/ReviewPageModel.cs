namespace FinanceManager.WebApp.Models;

using FinanceManager.Application.DTOs;
using FinanceManager.Domain.Entities;
using FinanceManager.WebApp.Models.Base;

public class ReviewPageModel
{
    public ValidationModel Validation { get; set; } = new();
    public PaginationModel<FilterQuery, ReviewGroup>  ReviewPagination { get; set; } = new() { Query = new() { FilterStatus = ReviewStatus.Unreviewed} };
    public PaginationModel<FilterQuery, TransactionSummary> TransferPagination { get; set; } = new();
    public PaginationModel<FilterQuery, TransactionSummary> ReimbursePagination { get; set; } = new();

    public ReviewGroup? CurrentGroup { get; set; }
    public ReviewTransaction? CurrentTransaction { get; set; }

    public List<Category> Categories { get; set; } = [];
    public List<Category> IncomeCategories => [.. Categories.Where(c => c.Group.IsIncome)];
    public List<Category> ExpenseCategories => [.. Categories.Where(c => !c.Group.IsIncome)];

    public bool HasMemories { get; set; } = true;
    public bool IsAutoAssignmentEnabled { get; set; } = true;

    public bool IsTransferSearchOpen { get; set; } = false;
    public bool IsReimburseSearchOpen { get; set; } = false;

    public string AmountKey = "amount";
    public string CategoryKey = "category";

    public void Clean()
    {
        CurrentGroup = null;
        CurrentTransaction = null;
        IsTransferSearchOpen = false;
        IsReimburseSearchOpen = false;
    }

    public async Task SetReimbursement(TransactionSummary transaction)
    {
        if (CurrentTransaction is null) return;

        CurrentTransaction.Reimburses = transaction;
        Clean();
    }

    public async Task SetTransfer(TransactionSummary transaction)
    {
        if (CurrentGroup is null) return;

        CurrentGroup.Transfers = transaction;
        Clean();
    }

    public bool ValidateGroup(ReviewGroup group)
    {
        Validation.ClearValidation();

        foreach (var transaction in group.Transactions)
        {
            if (transaction.Category is null && group.Transfers is null)
            {
                Validation.SetError(transaction.Id, "category", "A category is required");
            }
            if (transaction.Amount == 0)
            {
                Validation.SetError(transaction.Id, "amount", "Amount must not be zero");
            }
        }

        return !Validation.HasError;
    }

    public async Task OnFindReimbursement(ReviewGroup group, ReviewTransaction transaction)
    {
        CurrentGroup = group;
        CurrentTransaction = transaction;
        IsReimburseSearchOpen = true;

        ReimbursePagination.Query.FilterStatus = ReviewStatus.Reviewed;

        await ReimbursePagination.UpdateResults();
    }

    public async Task OnFindTransfer(ReviewGroup group)
    {
        CurrentGroup = group;
        IsTransferSearchOpen = true;

        TransferPagination.Query.FilterAmountMax = -group.InitalTransaction.Amount;
        TransferPagination.Query.FilterAmountMin = -group.InitalTransaction.Amount;

        await TransferPagination.UpdateResults();
    }

    public void EndFindReimbursement()
    {
        IsReimburseSearchOpen = false;
        Clean();
    }

    public void EndFindTransfer()
    {
        IsTransferSearchOpen = false;
        Clean();
    }

    public void RevertAutoAssign()
    {
        var unassignedCount = 0;

        foreach (var group in ReviewPagination.Result.Items)
        {
            foreach (var transaction in group.Transactions)
            {
                if (transaction.AutoCategorised)
                {
                    transaction.Category = null;
                    transaction.AutoCategorised = false;
                    unassignedCount++;
                }
            }
        }

        if (unassignedCount > 0)
        {
            Validation.SetValidationState(ValidationType.Success, $"Reverted {unassignedCount} assignments");
        }
    }
}