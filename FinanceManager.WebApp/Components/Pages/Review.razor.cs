namespace FinanceManager.WebApp.Components.Pages;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Enums;
using FinanceManager.WebApp.Components.Features.Review;
using FinanceManager.WebApp.Models;

public partial class Review : PageBase
{
    public DataGridModel<FilterQuery, ReviewGroup> Data { get; set; } = new(20);
    public DataGridModel<FilterQuery, TransactionSummary> TransferData { get; set; } = new(15);
    public DataGridModel<FilterQuery, TransactionSummary> ReimburseData { get; set; } = new(15);

    public ReviewGroup? CurrentGroup { get; set; }
    public ReviewTransaction? CurrentTransaction { get; set; }

    public IReadOnlyList<CategorySummary> Categories { get; set; } = [];

    public FindTransferModal TransferModal { get; set; } = new();
    public FindReimburseModal ReimburseModal { get; set; } = new();

    public bool IsFirstAutoAssign { get; set; } = true;
    public bool IsAutoAssignEnabled { get; set; } = true;

    protected override async Task OnInitializedAsync()
    {
        Validation.Messenger = Messenger;

        Data.GetDataFunc = GetData;
        Data.UpdateViewState = StateHasChanged;
        Data.Query.SortDescending = false;

        TransferData.GetDataFunc = GetTransferData;
        ReimburseData.GetDataFunc = GetReimburseData;

        Categories = (await UseCases.GetCategoryListAsync()).Categories;
    }

    private async Task<PagedResult<ReviewGroup>> GetData(FilterQuery query)
    {
        var result = (await UseCases.GetPagedReviewAsync(query)).Page;

        if (IsAutoAssignEnabled)
        {
            var autoAssignResult = await UseCases.AutoCategoriseAsync(result.Items, Categories);
            if (autoAssignResult.AssignedCount > 0)
            {
                Validation.SetSuccess($"Auto assigned {autoAssignResult.AssignedCount} transactions");
            }
            else if (Data.HasResults && IsFirstAutoAssign)
            {
                Validation.SetInfo("Not enough information to assign categories");
                IsFirstAutoAssign = false;
            }
        }

        return result;
    }

    private async Task<PagedResult<TransactionSummary>> GetTransferData(FilterQuery query)
    {
        var amount = CurrentGroup?.InitialTransaction.Amount ?? 0;
        return (await UseCases.GetTransferCandidatesAsync(query, amount)).Page;
    }

    private async Task<PagedResult<TransactionSummary>> GetReimburseData(FilterQuery query)
    {
        return (await UseCases.GetReimbursementCandidatesAsync(query)).Page;
    }

    private async Task SaveGroup(ReviewGroup group, bool showMessage = true)
    {
        Validation.Clear();

        var validationResult = await UseCases.ValidateReviewGroupAsync(group);
        if (!validationResult.IsSuccess)
        {
            Validation.SetErrors(validationResult.Errors);
            return;
        }

        var saveResult = await UseCases.SaveReviewAsync(group);
        if (!saveResult.IsSuccess)
        {
            Validation.SetErrors(saveResult.Errors);
            return;
        }

        await Data.UpdateAsync();
        if (showMessage) Validation.SetSuccess("Activity saved successfully");
    }

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

    public void SetCategory(ReviewTransaction transaction, CategorySummary? value)
    {
        Validation.ClearValidationItem(transaction.EntityId, ValidationField.Category);
        transaction.Category = value;
        transaction.IsAutoCategorised = false;
    }

    public void SetDate(ReviewGroup group, ReviewTransaction transaction, DateTime value)
    {
        Validation.ClearValidationItem(transaction.EntityId, ValidationField.Date);
        var result = UseCases.BackdateTransactionAsync(transaction, value, group.InitialTransaction.Date).Result;
        
        if (!result.IsSuccess) Validation.SetErrors(result.Errors);
    }

    public async Task SetAmount(ReviewGroup group, ReviewTransaction transaction, decimal value)
    {
        Validation.ClearValidationItem(transaction.EntityId, ValidationField.Amount);
        var result = await UseCases.UpdateTransactionAmountAsync(transaction, value, group);
      
        if (!result.IsSuccess) Validation.SetErrors(result.Errors);
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