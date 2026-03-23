namespace FinanceManager.Application.UseCases.Transactions;

using FinanceManager.Application.Common;
using FinanceManager.Application.DTOs;
using FinanceManager.Application.Enums;
using FinanceManager.Domain.Entities;

public sealed record GetChart1DataResult(
    List<TransactionSummary> Transactions,
    List<Category> IncomeCategories,
    List<Category> ExpenseCategories,
    decimal[]? BudgetSeries,
    decimal[]? IncomeBudgetSeries,
    decimal[]? ExpenseBudgetSeries
) : UseCaseResult;

public sealed class GetChart1Data(TransactionHelper transactionHelper, GetCategoriesForTransactions getCategoriesForTransactions)
{
    public async Task<GetChart1DataResult> ExecuteAsync(ScopedPeriod period, Guid? drilldownGroupId, TransactionsGraphMode mode)
    {
        var query = new FilterQuery
        {
            FilterDateFrom = period.StartDate.ToDateTime(TimeOnly.MinValue),
            FilterDateTo = period.EndDate.ToDateTime(TimeOnly.MinValue),
            FilterStatus = ReviewStatus.Reviewed
        };

        var transactions = await transactionHelper.GetTransactionsForRange(query);
        var categories = (await getCategoriesForTransactions.ExecuteAsync()).Categories;

        var incomeCategories = categories.Where(c => c.Group.IsIncome).ToList();
        var expenseCategories = categories.Where(c => !c.Group.IsIncome).ToList();

        if (drilldownGroupId is Guid id)
        {
            incomeCategories = [.. incomeCategories.Where(c => c.GroupId == id)];
            expenseCategories = [.. expenseCategories.Where(c => c.GroupId == id)];
        }

        decimal[]? budgetSeriesData = null;
        decimal[]? budgetIncomeSeriesData = null;
        decimal[]? budgetExpenseSeriesData = null;

        if (mode == TransactionsGraphMode.Net)
        {
            if (drilldownGroupId is null || incomeCategories.Count != 0)
            {
                budgetIncomeSeriesData = await transactionHelper.GetBudgetPerMonthForCategories(period, incomeCategories, false);
            }
            if (drilldownGroupId is null || expenseCategories.Count != 0)
            {
                budgetExpenseSeriesData = await transactionHelper.GetBudgetPerMonthForCategories(period, expenseCategories, true);
            }
        }
        else
        {
            var relevantCategories = mode == TransactionsGraphMode.Income ? incomeCategories : expenseCategories;
            budgetSeriesData = await transactionHelper.GetBudgetPerMonthForCategories(period, relevantCategories, false);
        }

        return new GetChart1DataResult(transactions, incomeCategories, expenseCategories, budgetSeriesData, budgetIncomeSeriesData, budgetExpenseSeriesData);
    }
}
