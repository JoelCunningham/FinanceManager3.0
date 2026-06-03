namespace FinanceManager.Application.UseCases;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Enums;
using FinanceManager.Application.UseCases.Dashboard;
using FinanceManager.Application.UseCases.Budget;
using FinanceManager.Application.UseCases.Categories;
using FinanceManager.Application.UseCases.Import;
using FinanceManager.Application.UseCases.Review;
using FinanceManager.Application.UseCases.Transactions;
using FinanceManager.Application.UseCases.Transfers;
using FinanceManager.Domain.Enums;

public class UseCases(
    AutoAssignCategories autoAssignCategories, 
    BackdateTransaction backdateTransaction, 
    DeleteBudgetEntry deleteBudgetEntry,
    DeleteCategory deleteCategory,
    DeleteCategoryGroup deleteCategoryGroup,
    GetBudgetScopes getBudgetScopes, 
    GetCategoryGroupList getCategoryGroupList,
    GetCategoryList getCategoryList,
    GetDashboardData getDashboardData,
    GetChart1Data getChart1Data, 
    GetChart2Data getChart2Data, 
    GetPagedBudget getBudgetPage, 
    GetPagedReview getPagedReview, 
    GetPagedTransactions getPagedTransactions, 
    GetPagedTransfers getPagedTransfers,
    GetParsers getParsers,
    GetReimbursementCandidates getReimbursementCandidates,
    GetReviewGroup getReviewGroup,
    GetTransactionDetails getTransactionDetails,
    GetTransferCandidates getTransferCandidates,
    GetUniqueAccounts getUniqueAccounts,
    ParseFile parseFile,
    SaveBudget saveBudget,
    SaveBudgetEntry saveBudgetEntry,
    SaveImport saveImport,
    SaveReview saveReview,
    SaveCategoryEdit saveCategoryEdit,
    SaveCategoryGroupEdit saveCategoryGroupEdit,
    SaveTransactionEdit saveTransactionEdit,
    SeparateTransfer separateTransfer,
    UpdateTransactionAmount updateTransactionAmount,
    ValidateReviewGroup validateReviewGroup,
    ValidateTransactionEdit validateTransactionEdit
)
{
    public GetParsersResult GetParsers() => getParsers.Execute();
    public Task DeleteBudgetEntryAsync(Guid id) => deleteBudgetEntry.ExecuteAsync(id);
    public Task SaveBudgetEntryAsync(BudgetCellEntry entry, int year, bool isEditing) => saveBudgetEntry.ExecuteAsync(entry, year, isEditing);
    public Task<AutoCategoriseResult> AutoCategoriseAsync(IEnumerable<ReviewGroup> groups, IEnumerable<CategorySummary> categories) => autoAssignCategories.ExecuteAsync(groups, categories);
    public Task<BackdateTransactionResult> BackdateTransactionAsync(TransactionSummary transaction, DateTime date, DateTime initialDate) => backdateTransaction.ExecuteAsync(transaction, date, initialDate);
    public Task<DeleteCategoryResult> DeleteCategoryAsync(Guid id) => deleteCategory.ExecuteAsync(id);
    public Task<DeleteCategoryGroupResult> DeleteCategoryGroupAsync(Guid id) => deleteCategoryGroup.ExecuteAsync(id);
    public Task<GetBudgetScopesResult> GetBudgetScopesAsync(ScopedRange range) => getBudgetScopes.ExecuteAsync(range);
    public Task<GetCategoryGroupsResult> GetCategoryGroupsAsync() => getCategoryGroupList.ExecuteAsync();
    public Task<GetCategoryListResult> GetCategoryListAsync() => getCategoryList.ExecuteAsync();
    public Task<GetDashboardDataResult> GetDashboardDataAsync() => getDashboardData.ExecuteAsync();
    public Task<GetChart1DataResult> GetChart1DataAsync(ScopedRange range, Guid? drilldownGroupId, TransactionsGraphMode mode) => getChart1Data.ExecuteAsync(range, drilldownGroupId, mode);
    public Task<GetChart2DataResult> GetChart2DataAsync(ScopedRange range, Guid? drilldownGroupId, TransactionsGraphMode mode) => getChart2Data.ExecuteAsync(range, drilldownGroupId, mode);
    public Task<GetPagedBudgetResult> GetPagedBudgetAsync(int year, BudgetGridMode mode = BudgetGridMode.Net) => getBudgetPage.ExecuteAsync(year, mode);
    public Task<GetPagedReviewResult> GetPagedReviewAsync(FilterQuery query) => getPagedReview.ExecuteAsync(query);
    public Task<GetPagedTransactionsResult> GetPagedTransactionsAsync(FilterQuery query) => getPagedTransactions.ExecuteAsync(query);
    public Task<GetPagedTransfersResult> GetPagedTransfersAsync(FilterQuery query) => getPagedTransfers.ExecuteAsync(query);
    public Task<GetReimbursementCandidatesResult> GetReimbursementCandidatesAsync(FilterQuery query) => getReimbursementCandidates.ExecuteAsync(query);
    public Task<GetReviewGroupResult> GetReviewGroupAsync(Guid transactionId) => getReviewGroup.ExecuteAsync(transactionId);
    public Task<GetTransactionDetailsResult> GetTransactionDetailsAsync(Guid transactionId) => getTransactionDetails.ExecuteAsync(transactionId);
    public Task<GetTransferCandidatesResult> GetTransferCandidatesAsync(FilterQuery query, decimal amount) => getTransferCandidates.ExecuteAsync(query, amount);
    public Task<GetUniqueAccountsResult> GetUniqueAccountsAsync() => getUniqueAccounts.ExecuteAsync();
    public Task<ImportSaveResult> SaveImportAsync(IEnumerable<ParsedTransaction> parsedTransactions) => saveImport.ExecuteAsync(parsedTransactions);
    public Task<ParseFileResult> ParseFileAsync(Stream file, string bank, string extension) => parseFile.ExecuteAsync(file, bank, extension);
    public Task<SaveBudgetResult> SaveBudgetAsync(int year, BudgetScope scope, bool isEditing) => saveBudget.ExecuteAsync(year, scope, isEditing);
    public Task<SaveReviewResult> SaveReviewAsync(ReviewGroup group) => saveReview.ExecuteAsync(group);
    public Task<SaveCategoryEditResult> SaveCategoryEditAsync(CategorySummary category) => saveCategoryEdit.ExecuteAsync(category);
    public Task<SaveCategoryGroupEditResult> SaveCategoryGroupEditAsync(CategoryGroupSummary group) => saveCategoryGroupEdit.ExecuteAsync(group);
    public Task<SaveTransactionEditResult> SaveTransactionEditAsync(TransactionSummary transaction) => saveTransactionEdit.ExecuteAsync(transaction);
    public Task<SeparateTransferResult> SeparateTransferAsync(Guid transferId) => separateTransfer.ExecuteAsync(transferId);
    public Task<UpdateTransactionAmountResult> UpdateTransactionAmountAsync(ReviewTransaction transaction, decimal amount, ReviewGroup group) => updateTransactionAmount.ExecuteAsync(transaction, amount, group);
    public Task<ValidateReviewGroupResult> ValidateReviewGroupAsync(ReviewGroup group) => validateReviewGroup.ExecuteAsync(group);
    public Task<ValidateTransactionEditResult> ValidateTransactionEditAsync(TransactionSummary transaction) => validateTransactionEdit.ExecuteAsync(transaction);
}
