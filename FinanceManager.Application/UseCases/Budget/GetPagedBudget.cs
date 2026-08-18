namespace FinanceManager.Application.UseCases.Budget;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Enums;
using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Entities;
using FinanceManager.Domain.Utilities;

public sealed record GetPagedBudgetResult(
    BudgetYearSummary? BudgetYear,
    IReadOnlyList<BudgetColumn> Columns,
    IReadOnlyList<CategorySummary> Categories,
    decimal TotalBudget,
    decimal TotalIncome,
    decimal TotalExpense
) : UseCaseResult;

public sealed class GetPagedBudget(IBudgetEntryRepository budgetEntryRepository, ICategoryRepository categoryRepository, IBudgetYearRepository budgetYearRepository, ITransactionRepository transactionRepository)
{
    public async Task<GetPagedBudgetResult> ExecuteAsync(int year, BudgetGridMode mode = BudgetGridMode.Net)
    {
        var budgetYear = await budgetYearRepository.GetByYearAsync(year);
        var categories = await categoryRepository.GetAllAsync();

        var categorySummaries = categories.Select(CategorySummary.FromCategory).ToList();

        if (budgetYear == null)
        {
            return new GetPagedBudgetResult(null, [], categorySummaries, 0m, 0m, 0m);
        }
        else
        {
            var columns = await BuildColumns(budgetYear, categorySummaries, mode);
            var budgets = (await budgetEntryRepository.GetByBudgetYearAsync(budgetYear.Id)).ToList();
            var actuals = (await transactionRepository.GetTransactionsAsync(new FilterQuery { FilterDateFrom = new DateTime(year, 1, 1), FilterDateTo = new DateTime(year, 12, 31) })).ToList();

            var budgetTotals = budgets.Aggregate(new BudgetTotals(), (current, entry) => current.Add(entry));
            var populatedColumns = PopulateColumns(columns, budgets, actuals);
            var summary = BudgetYearSummary.FromBudgetYear(budgetYear);

            return new GetPagedBudgetResult(summary, populatedColumns, categorySummaries, budgetTotals.Income - budgetTotals.Expense, budgetTotals.Income, budgetTotals.Expense);
        }
    }

    private sealed record BudgetTotals(decimal Income = 0m, decimal Expense = 0m)
    {
        public BudgetTotals Add(BudgetEntry entry)
        {
            var amount = entry.Amount * entry.Length;
            return entry.Category.Group.IsIncome ? this with { Income = Income + amount } : this with { Expense = Expense + amount };
        }
    }

    private static async Task<List<BudgetColumn>> BuildColumns(BudgetYear budgetYear, IEnumerable<CategorySummary> categories, BudgetGridMode mode)
    {
        var columns = new List<BudgetColumn>();

        var scopeStart = ScopeHelper.GetIsoYearStart(budgetYear.Scope, budgetYear.Year);
        var periodsCount = ScopeHelper.GetPeriodCount(budgetYear.Scope, budgetYear.Year);

        for (var i = 0; i < periodsCount; i++)
        {
            var cells = new List<BudgetCell>();

            var columnStart = ScopeHelper.GetPeriodStart(budgetYear.Scope, scopeStart, i);

            foreach (var category in categories)
            {
                if (mode == BudgetGridMode.Income && !category.IsIncome) continue;
                if (mode == BudgetGridMode.Expense && category.IsIncome) continue;

                cells.Add(new BudgetCell(budgetYear.Scope, category, columnStart));
            }

            columns.Add(new BudgetColumn(columnStart, budgetYear.Scope, cells));
        }

        return columns;
    }

    private static List<BudgetColumn> PopulateColumns(List<BudgetColumn> columns, List<BudgetEntry> budgets, List<Transaction> actuals)
    {
        foreach (var column in columns)
        {
            foreach (var cell in column.Cells)
            {
                cell.BudgetEntries = [.. budgets
                    .Where(e => e.Category.Id == cell.Category.Id && e.StartDate <= column.StartDate && e.EndDate >= column.StartDate)
                    .Select(BudgetCellEntry.FromBudgetEntry)];

                cell.Transactions = [.. actuals
                    .Where(t => t.CategoryId == cell.Category.Id && t.Date >= column.StartDate.ToDateTime(TimeOnly.MinValue) && t.Date <= column.EndDate.ToDateTime(TimeOnly.MaxValue))
                    .Select(TransactionSummary.FromTransaction)];
            }
        }

        return columns;
    }
}
