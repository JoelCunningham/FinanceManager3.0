namespace FinanceManager.Application.UseCases.Budget;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Enums;
using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Entities;
using FinanceManager.Domain.Enums;
using FinanceManager.Domain.Utilities;
using System.Data.Common;

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
            var columns = await BuildColumns(budgetYear, categorySummaries);
            var entries = (await budgetEntryRepository.GetByBudgetYearAsync(budgetYear.Id)).ToList();
            var totals = entries.Aggregate(new BudgetTotals(), (current, entry) => current.Add(entry));

            entries = mode switch
            {
                BudgetGridMode.Income => [.. entries.Where(entry => entry.Category.Group.IsIncome)],
                BudgetGridMode.Expense => [.. entries.Where(entry => !entry.Category.Group.IsIncome)],
                _ => entries
            };

            var populatedColumns = PopulateColumns(columns, entries);
            var summary = BudgetYearSummary.FromBudgetYear(budgetYear);

            return new GetPagedBudgetResult(summary, populatedColumns, categorySummaries, totals.Income - totals.Expense, totals.Income, totals.Expense);
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

    private async Task<List<BudgetColumn>> BuildColumns(BudgetYear budgetYear, IEnumerable<CategorySummary> categories)
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
                var query = new FilterQuery
                {
                    FilterDateFrom = columnStart.ToDateTime(TimeOnly.MinValue),
                    FilterDateTo = ScopeHelper.GetPeriodEnd(budgetYear.Scope, columnStart).ToDateTime(TimeOnly.MaxValue),
                    FilterCategory = category
                };
                var realAmount = (await transactionRepository.GetTransactionsAsync(query)).Sum(t => t.Amount);

                cells.Add(new BudgetCell(budgetYear.Scope, i, category, columnStart, realAmount));
            }

            columns.Add(new BudgetColumn(i, columnStart, budgetYear.Scope, cells));
        }

        return columns;
    }

    private static List<BudgetColumn> PopulateColumns(List<BudgetColumn> columns, List<BudgetEntry> entries)
    {
        entries = [.. entries.OrderByDescending(e => e.Category.Group.IsIncome).ThenBy(e => e.Category.Group.Name).ThenBy(e => e.Category.Name)];

        foreach (var entry in entries)
        {
            for (var i = 0; i < entry.Length; i++)
            {
                var currentPosition = entry.ScopePosition + i;

                var column = columns[currentPosition];
                var cell = column.Cells.FirstOrDefault(c => c.Category.Id == entry.Category.Id);

                if (cell is null) continue;
                cell.Entries = cell.Entries.Append(BudgetCellEntry.FromBudgetEntry(entry));
            }
        }

        return columns;
    }
}
