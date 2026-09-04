namespace FinanceManager.Application.UseCases.Statistics;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Enums;
using FinanceManager.Application.Interfaces;
using FinanceManager.Application.UseCases;
using FinanceManager.Application.UseCases.Categories;
using FinanceManager.Application.Utilities;
using System.Linq;

public sealed record GetChart1DataResult(
    List<TransactionSummary> Transactions,
    List<CategorySummary> IncomeCategories,
    List<CategorySummary> ExpenseCategories,
    IEnumerable<decimal> BudgetSeries,
    IEnumerable<decimal> IncomeBudgetSeries,
    IEnumerable<decimal> ExpenseBudgetSeries
) : UseCaseResult;

public sealed class GetChart1Data(ChartHelper chartHelper, GetCategories getCategoryList, ITransactionRepository transactionRepository)
{
    public async Task<GetChart1DataResult> ExecuteAsync(IEnumerable<ScopedPeriod> range, Guid? drilldownGroupId, ChartMode mode)
    {
        var query = new FilterQuery
        {
            FilterDateFrom = range.First().StartDate.ToDateTime(TimeOnly.MinValue),
            FilterDateTo = range.Last().EndDate.ToDateTime(TimeOnly.MaxValue),
            FilterStatus = ReviewStatus.Reviewed
        };

        var transactions = (await transactionRepository.GetTransactionsAsync(query)).Select(TransactionSummary.FromTransaction).ToList();
        var categories = (await getCategoryList.ExecuteAsync()).Categories;

        var incomeCategories = categories.Where(c => c.IsIncome).ToList();
        var expenseCategories = categories.Where(c => !c.IsIncome).ToList();

        if (drilldownGroupId is Guid id)
        {
            incomeCategories = [.. incomeCategories.Where(c => c.GroupId == id)];
            expenseCategories = [.. expenseCategories.Where(c => c.GroupId == id)];
        }

        IEnumerable<decimal> budgetSeriesData = [];
        IEnumerable<decimal> budgetIncomeSeriesData = [];
        IEnumerable<decimal> budgetExpenseSeriesData = [];

        if (mode == ChartMode.IncomeAndExpense)
        {
            if (drilldownGroupId is null || incomeCategories.Count != 0)
            {
                foreach (var category in incomeCategories)
                {
                    budgetIncomeSeriesData = budgetIncomeSeriesData.Concat(await chartHelper.GetBudgetsPerPeriod(category, range, false));
                }
            }
            if (drilldownGroupId is null || expenseCategories.Count != 0)
            {
                foreach (var category in expenseCategories)
                {
                    budgetExpenseSeriesData = budgetExpenseSeriesData.Concat(await chartHelper.GetBudgetsPerPeriod(category, range, true));
                }
            }
        }
        else
        {
            var relevantCategories = mode == ChartMode.Income ? incomeCategories : expenseCategories;
            foreach (var category in relevantCategories)
            {
                budgetSeriesData = budgetSeriesData.Concat(await chartHelper.GetBudgetsPerPeriod(category, range, mode == ChartMode.Expense));
            }
        }

        return new GetChart1DataResult(transactions, incomeCategories, expenseCategories, budgetSeriesData, budgetIncomeSeriesData, budgetExpenseSeriesData);
    }
}
