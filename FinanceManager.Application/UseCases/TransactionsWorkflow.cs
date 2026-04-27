namespace FinanceManager.Application.UseCases;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.UseCases.Categories;
using FinanceManager.Application.UseCases.Transactions;
using FinanceManager.Application.UseCases.Transfers;

public sealed class TransactionsWorkflow(GetTransactionsPage getPagedTransactions, GetUniqueAccounts getUniqueAccounts, GetTransactionDetails getTransactionDetails, SaveTransactionEdit saveTransactionEdit, ValidateTransactionEdit validateTransactionEdit, GetCategoryList getCategoryList)
{
    public Task<GetTransactionsPageResult> GetPagedTransactionsAsync(FilterQuery query) => getPagedTransactions.ExecuteAsync(query);
    public Task<GetUniqueAccountsResult> GetUniqueAccountsAsync() => getUniqueAccounts.ExecuteAsync();
    public Task<GetTransactionDetailsResult> GetDetailsAsync(Guid transactionId) => getTransactionDetails.ExecuteAsync(transactionId);
    public Task<ValidateTransactionEditResult> ValidateTransactionEditAsync(TransactionSummary transaction) => validateTransactionEdit.ExecuteAsync(transaction);
    public Task<SaveTransactionEditResult> SaveTransactionEditAsync(TransactionSummary transaction) => saveTransactionEdit.ExecuteAsync(transaction);
    public Task<GetCategoryListResult> GetCategoriesAsync() => getCategoryList.ExecuteAsync();
}
