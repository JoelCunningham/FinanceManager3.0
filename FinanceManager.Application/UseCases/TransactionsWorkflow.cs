namespace FinanceManager.Application.UseCases;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.UseCases.Transactions;
using FinanceManager.Domain.Entities;

public sealed class TransactionsWorkflow(GetTransactionsForRange getTransactionsForRange, GetCategoriesForTransactions getCategoriesForTransactions, GetBudgetPerMonthForCategories getBudgetPerMonthForCategories, GetBudgetPerLabel getBudgetPerLabel, GetBudgetScopes getBudgetScopes)
{
    public Task<GetTransactionsForRangeResult> GetTransactionsAsync(FilterQuery query) => getTransactionsForRange.ExecuteAsync(query);
    public Task<GetCategoriesForTransactionsResult> GetCategoriesAsync() => getCategoriesForTransactions.ExecuteAsync();
    public Task<GetBudgetPerMonthForCategoriesResult> GetBudgetPerMonthAsync(ScopedPeriod period, List<Category> categories, bool asExpense) => getBudgetPerMonthForCategories.ExecuteAsync(period, categories, asExpense);
    public Task<GetBudgetPerLabelResult> GetBudgetPerLabelAsync(DateOnly start, DateOnly end, List<Category> categories, bool isCategoryDrilldown) => getBudgetPerLabel.ExecuteAsync(start, end, categories, isCategoryDrilldown);
    public Task<GetBudgetScopesResult> GetBudgetScopesAsync(ScopedPeriod period) => getBudgetScopes.ExecuteAsync(period);
}
