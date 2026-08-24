namespace FinanceManager.Application.UseCases;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Enums;
using FinanceManager.Application.Models;
using FinanceManager.Application.UseCases.Auth;
using FinanceManager.Application.UseCases.Budget;
using FinanceManager.Application.UseCases.Categories;
using FinanceManager.Application.UseCases.Dashboard;
using FinanceManager.Application.UseCases.Import;
using FinanceManager.Application.UseCases.Review;
using FinanceManager.Application.UseCases.Settings;
using FinanceManager.Application.UseCases.Statistics;
using FinanceManager.Application.UseCases.Transactions;
using FinanceManager.Application.UseCases.Transfers;
using FinanceManager.Domain.Enums;

public class UseCases(
    AutoAssignCategories autoAssignCategories,
    ConfirmEmail confirmEmail,
    DeleteBudget deleteBudget,
    DeleteBudgetEntry deleteBudgetEntry,
    DeleteCategory deleteCategory,
    DeleteCategoryGroup deleteCategoryGroup,
    DeleteUser deleteUser,
    ExportTransactions exportTransactions,
    ForgotPassword forgotPassword,
    GetAvailablePeriods getAvailablePeriods,
    GetBudgetYears getBudgetYears,
    GetCategories getCategories,
    GetCategoryGraph getCategoryGraph,
    GetCategoryGroupDetails getCategoryGroupDetails,
    GetCategoryGroups getCategoryGroups,
    GetChart1Data getChart1Data,
    GetChart2Data getChart2Data,
    GetDashboardData getDashboardData,
    GetPagedBudget getBudgetPage,
    GetPagedReview getPagedReview,
    GetPagedTransactions getPagedTransactions,
    GetPagedTransfers getPagedTransfers,
    GetParsers getParsers,
    GetProfile getProfile,
    GetReimbursementCandidates getReimbursementCandidates,
    GetReviewGroup getReviewGroup,
    GetTransactionDetails getTransactionDetails,
    GetTransferCandidates getTransferCandidates,
    GetUniqueAccounts getUniqueAccounts,
    GetUserStatus getUserStatus,
    LoginUser loginUser,
    ParseFile parseFile,
    RegisterUser registerUser,
    ResetPassword resetPassword,
    SaveBudget saveBudget,
    SaveBudgetEntry saveBudgetEntry,
    SaveCategoryEdit saveCategoryEdit,
    SaveCategoryGroupEdit saveCategoryGroupEdit,
    SaveImport saveImport,
    SaveReview saveReview,
    SaveTransactionEdit saveTransactionEdit,
    SendMfaCode sendMfaCode,
    SeparateTransfer separateTransfer,
    UpdateUserEmail updateUserEmail,
    UpdateUserName updateUserName,
    UpdateUserPassword updateUserPassword,
    ValidateResetToken validateResetToken,
    ValidateTransactionEdit validateTransactionEdit
)
{
    public Task<AutoCategoriseResult> AutoCategoriseAsync(IEnumerable<ReviewGroup> groups, IEnumerable<CategorySummary> categories) => autoAssignCategories.ExecuteAsync(groups, categories);
    public Task<ConfirmEmailResult> ConfirmEmailAsync(ConfirmEmailModel model) => confirmEmail.ExecuteAsync(model);
    public static Task<BackdateTransactionResult> BackdateTransactionAsync(TransactionSummary transaction, DateTime date, DateTime initialDate) => BackdateTransaction.ExecuteAsync(transaction, date, initialDate);
    public Task<DeleteBudgetResult> DeleteBudgetAsync(Guid id) => deleteBudget.ExecuteAsync(id);
    public Task<DeleteBudgetEntryResult> DeleteBudgetEntryAsync(Guid id) => deleteBudgetEntry.ExecuteAsync(id);
    public Task<DeleteCategoryResult> DeleteCategoryAsync(Guid id) => deleteCategory.ExecuteAsync(id);
    public Task<DeleteCategoryGroupResult> DeleteCategoryGroupAsync(Guid id) => deleteCategoryGroup.ExecuteAsync(id);
    public Task<DeleteUserResult> DeleteUserAsync() => deleteUser.ExecuteAsync();
    public Task<ExportTransactionsResult> ExportTransactionsAsync(ExportType exportType) => exportTransactions.ExecuteAsync(exportType);
    public Task<ForgotPasswordResult> ForgotPasswordAsync(ForgotPasswordModel model) => forgotPassword.ExecuteAsync(model);
    public Task<GetAvailablePeriodsResult> GetAvailablePeriodsAsync(Guid? categoryGroupId = null, bool allowFuture = false) => getAvailablePeriods.ExecuteAsync(categoryGroupId, allowFuture);
    public Task<GetBudgetYearsResult> GetBudgetYearsAsync() => getBudgetYears.ExecuteAsync();
    public Task<GetCategoriesResult> GetCategoriesAsync() => getCategories.ExecuteAsync();
    public Task<GetCategoryGraphResult> GetCategoryGraphAsync(IEnumerable<CategorySummary> categories, IEnumerable<ScopedPeriod> periods) => getCategoryGraph.ExecuteAsync(categories, periods);
    public Task<GetCategoryGroupDetailsResult> GetCategoryGroupDetailsAsync(string groupName, ScopedPeriod period) => getCategoryGroupDetails.ExecuteAsync(groupName, period);
    public Task<GetCategoryGroupsResult> GetCategoryGroupsAsync() => getCategoryGroups.ExecuteAsync();
    public Task<GetChart1DataResult> GetChart1DataAsync(IEnumerable<ScopedPeriod> range, Guid? drilldownGroupId, TransactionsGraphMode mode) => getChart1Data.ExecuteAsync(range, drilldownGroupId, mode);
    public Task<GetChart2DataResult> GetChart2DataAsync(IEnumerable<ScopedPeriod> range, Guid? drilldownGroupId, TransactionsGraphMode mode) => getChart2Data.ExecuteAsync(range, drilldownGroupId, mode);
    public Task<GetDashboardDataResult> GetDashboardDataAsync(ScopedPeriod? period = null) => getDashboardData.ExecuteAsync(period);
    public Task<GetPagedBudgetResult> GetPagedBudgetAsync(int year, BudgetGridMode mode = BudgetGridMode.Net) => getBudgetPage.ExecuteAsync(year, mode);
    public Task<GetPagedReviewResult> GetPagedReviewAsync(FilterQuery query) => getPagedReview.ExecuteAsync(query);
    public Task<GetPagedTransactionsResult> GetPagedTransactionsAsync(FilterQuery query) => getPagedTransactions.ExecuteAsync(query);
    public Task<GetPagedTransfersResult> GetPagedTransfersAsync(FilterQuery query) => getPagedTransfers.ExecuteAsync(query);
    public Task<GetReimbursementCandidatesResult> GetReimbursementCandidatesAsync(FilterQuery query) => getReimbursementCandidates.ExecuteAsync(query);
    public Task<GetReviewGroupResult> GetReviewGroupAsync(Guid transactionId) => getReviewGroup.ExecuteAsync(transactionId);
    public Task<GetTransactionDetailsResult> GetTransactionDetailsAsync(Guid transactionId) => getTransactionDetails.ExecuteAsync(transactionId);
    public Task<GetTransferCandidatesResult> GetTransferCandidatesAsync(FilterQuery query, decimal amount) => getTransferCandidates.ExecuteAsync(query, amount);
    public Task<GetUniqueAccountsResult> GetUniqueAccountsAsync() => getUniqueAccounts.ExecuteAsync();
    public Task<GetUserStatusResult> GetUserStatusAsync(DateOnly? staleCutoff = null) => getUserStatus.ExecuteAsync(staleCutoff);
    public Task<GetParsersResult> GetParsersAsync() => getParsers.ExecuteAsync();
    public Task<GetProfileResult> GetProfileAsync() => getProfile.ExecuteAsync();
    public Task<LoginUserResult> LoginUserAsync(LoginModel model) => loginUser.ExecuteAsync(model);
    public static Task<LogoutUserResult> LogoutUserAsync() => LogoutUser.ExecuteAsync();
    public Task<ParseFileResult> ParseFileAsync(Stream file, string bank, string extension) => parseFile.ExecuteAsync(file, bank, extension);
    public Task<RegisterUserResult> RegisterUserAsync(RegisterUserModel model) => registerUser.ExecuteAsync(model);
    public Task<ResetPasswordResult> ResetPasswordAsync(ResetPasswordModel model) => resetPassword.ExecuteAsync(model);
    public Task<SaveBudgetEntryResult> SaveBudgetEntryAsync(BudgetCellEntry entry, int year, bool isEditing) => saveBudgetEntry.ExecuteAsync(entry, year, isEditing);
    public Task<SaveBudgetResult> SaveBudgetAsync(int year, BudgetScope scope, bool isEditing) => saveBudget.ExecuteAsync(year, scope, isEditing);
    public Task<SaveCategoryEditResult> SaveCategoryEditAsync(CategorySummary category) => saveCategoryEdit.ExecuteAsync(category);
    public Task<SaveCategoryGroupEditResult> SaveCategoryGroupEditAsync(CategoryGroupSummary group) => saveCategoryGroupEdit.ExecuteAsync(group);
    public Task<SaveImportResult> SaveImportAsync(IEnumerable<ParsedTransaction> parsedTransactions) => saveImport.ExecuteAsync(parsedTransactions);
    public Task<SaveReviewResult> SaveReviewAsync(ReviewGroup group) => saveReview.ExecuteAsync(group);
    public Task<SaveTransactionEditResult> SaveTransactionEditAsync(TransactionSummary transaction) => saveTransactionEdit.ExecuteAsync(transaction);
    public Task<SeparateTransferResult> SeparateTransferAsync(Guid transferId) => separateTransfer.ExecuteAsync(transferId);
    public Task<SendMfaCodeResult> SendMfaCodeAsync(string name, string address) => sendMfaCode.ExecuteAsync(name, address);
    public static Task<UpdateTransactionAmountResult> UpdateTransactionAmountAsync(ReviewTransaction transaction, decimal amount, ReviewGroup group) => UpdateTransactionAmount.ExecuteAsync(transaction, amount, group);
    public Task<UpdateUserEmailResult> UpdateUserEmailAsync(ProfileModel model) => updateUserEmail.ExecuteAsync(model);
    public Task<UpdateUserNameResult> UpdateUserNameAsync(ProfileModel model) => updateUserName.ExecuteAsync(model);
    public Task<UpdateUserPasswordResult> UpdateUserPasswordAsync(PasswordModel model) => updateUserPassword.ExecuteAsync(model);
    public static Task<ValidateReviewGroupResult> ValidateReviewGroupAsync(ReviewGroup group) => ValidateReviewGroup.ExecuteAsync(group);
    public Task<ValidateResetTokenResult> ValidateResetTokenAsync(ResetPasswordModel model) => validateResetToken.ExecuteAsync(model);
    public Task<ValidateTransactionEditResult> ValidateTransactionEditAsync(TransactionSummary transaction) => validateTransactionEdit.ExecuteAsync(transaction);
}
