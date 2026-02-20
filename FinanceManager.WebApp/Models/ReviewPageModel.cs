namespace FinanceManager.WebApp.Models;

using FinanceManager.Application.DTOs;
using FinanceManager.Domain.Entities;
using FinanceManager.WebApp.Models.Base;
using Havit.Blazor.Components.Web.Bootstrap;

public class ReviewPageModel
{
    public ValidationModel Validation { get; set; } = new();

    public DataGridModel<FilterQuery, ReviewGroup> Data { get; set; } = new(20);
    public DataGridModel<FilterQuery, TransactionSummary> TransferData { get; set; } = new(15);
    public DataGridModel<FilterQuery, TransactionSummary> ReimburseData { get; set; } = new(15);

    public ReviewGroup? CurrentGroup { get; set; }
    public ReviewTransaction? CurrentTransaction { get; set; }

    public List<Category> Categories { get; set; } = [];
    public List<Category> IncomeCategories => [.. Categories.Where(c => c.Group.IsIncome)];
    public List<Category> ExpenseCategories => [.. Categories.Where(c => !c.Group.IsIncome)];

    public HxModal TransferModal { get; set; } = new();
    public HxModal ReimburseModal { get; set; } = new();

    public bool IsFirstAutoAssign { get; set; } = true;
    public bool IsAutoAssignEnabled { get; set; } = true;

    public string AmountKey = "amount";
    public string CategoryKey = "category";

    public async Task SetReimburse(TransactionSummary transaction)
    {
        if (CurrentTransaction is null) return;
        CurrentTransaction.Reimburses = transaction;
        await ReimburseModal.HideAsync();

    }

    public async Task SetTransfer(TransactionSummary transaction)
    {
        if (CurrentGroup is null) return;
        CurrentGroup.Transfers = transaction;
        await TransferModal.HideAsync();
    }

    public void SetCategory(ReviewTransaction transaction, Category? value)
    {
        Validation.ClearValidationItem(transaction.Id, CategoryKey);
        transaction.Category = value;
        transaction.IsAutoCategorised = false;
    }

    public void SetAmount(ReviewGroup group, ReviewTransaction transaction, decimal value)
    {
        Validation.ClearValidationItem(transaction.Id, AmountKey);
        try
        {
            group.SetAmount(transaction, value);
        }
        catch (InvalidOperationException) 
        {
            Validation.SetError(transaction.Id, AmountKey, "Amount must be between 0 and the original amount");
        }
    }

    public bool ValidateGroup(ReviewGroup group)
    {
        Validation.ClearValidation();
        foreach (var transaction in group.Transactions)
        {
            if (transaction.Category is null && group.Transfers is null)
            {
                Validation.SetError(transaction.Id, CategoryKey, "A category is required");
            }
            if (transaction.Amount == 0)
            {
                Validation.SetError(transaction.Id, AmountKey, "Amount must not be zero");
            }
        }
        return Validation.Type != ValidationType.Error;
    }

    public async Task OnFindReimbursement(ReviewGroup group, ReviewTransaction transaction)
    {
        CurrentGroup = group;
        CurrentTransaction = transaction;
        await ReimburseModal.ShowAsync();
    }

    public async Task OnFindTransfer(ReviewGroup group)
    {
        CurrentGroup = group;
        await TransferModal.ShowAsync();
    }
}