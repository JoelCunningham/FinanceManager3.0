namespace FinanceManager.WebApp.Components.Pages;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.UseCases;
using FinanceManager.Domain.Entities;
using FinanceManager.WebApp.Components.Features.Review;
using FinanceManager.WebApp.Models;
using Havit.Blazor.Components.Web;
using Microsoft.AspNetCore.Components;

public partial class Review : ComponentBase
{
    [Inject] public ReviewWorkflow ReviewWorkflow { get; set; } = default!;
    [Inject] public IHxMessengerService Messenger { get; set; } = default!;

    public ValidationModel Validation { get; set; } = new();

    public DataGridModel<FilterQuery, ReviewGroup> Data { get; set; } = new(20);
    public DataGridModel<FilterQuery, TransactionSummary> TransferData { get; set; } = new(15);
    public DataGridModel<FilterQuery, TransactionSummary> ReimburseData { get; set; } = new(15);

    public ReviewGroup? CurrentGroup { get; set; }
    public ReviewTransaction? CurrentTransaction { get; set; }

    public List<Category> Categories { get; set; } = [];

    public FindTransferModal TransferModal { get; set; } = new();
    public FindReimburseModal ReimburseModal { get; set; } = new();

    public bool IsFirstAutoAssign { get; set; } = true;
    public bool IsAutoAssignEnabled { get; set; } = true;

    public readonly string AmountKey = "amount";
    public readonly string CategoryKey = "category";

    protected override async Task OnInitializedAsync()
    {
        Validation.Messenger = Messenger;

        Data.GetDataFunc = GetData;
        Data.UpdateViewState = StateHasChanged;

        TransferData.GetDataFunc = GetTransferData;
        ReimburseData.GetDataFunc = GetReimburseData;

        var cateogryDto = (await ReviewWorkflow.GetCategoriesAsync()).Categories;
        var groupDict = cateogryDto.GroupBy(c => c.GroupId).ToDictionary(g => g.Key, g => new CategoryGroup { Id = g.Key, Name = g.First().GroupName, IsIncome = g.First().IsIncome });
        Categories = [.. cateogryDto.Select(c => new Category { Id = c.Id, Name = c.Name, GroupId = c.GroupId, Group = groupDict[c.GroupId] })];
    }

    private async Task<PagedResult<ReviewGroup>> GetData(FilterQuery query)
    {
        query.FilterStatus = ReviewStatus.Unreviewed;
        var result = (await ReviewWorkflow.GetPageAsync(query)).Page;

        if (IsAutoAssignEnabled)
        {
            var autoAssignResult = await ReviewWorkflow.AutoCategoriseAsync(result.Items, Categories);
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
        var amount = CurrentGroup?.InitalTransaction.Amount ?? 0;
        return (await ReviewWorkflow.GetTransferCandidatesAsync(query, amount)).Page;
    }

    private async Task<PagedResult<TransactionSummary>> GetReimburseData(FilterQuery query)
    {
        return (await ReviewWorkflow.GetReimbursementCandidatesAsync(query)).Page;
    }

    private async Task SaveGroup(ReviewGroup group, bool showMessage = true)
    {
        Validation.Clear();

        var validationResult = await ReviewWorkflow.ValidateGroupAsync(group);
        if (!validationResult.IsValid)
        {
            foreach (var error in validationResult.Errors)
            {
                Validation.SetError(error.TransactionId, error.Key, error.Message);
            }
            return;
        }

        var saveResult = await ReviewWorkflow.SaveAsync(group);
        if (saveResult.IsSuccess)
        {
            await Data.UpdateAsync();
            if (showMessage) Validation.SetSuccess("Transaction saved successfully");
        }
        else
        {
            Validation.SetError(saveResult.ErrorMessage ?? "An unexpected error occurred. Please try again");
        }
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