namespace FinanceManager.Application.UseCases.Budget;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Enums;
using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Entities;
using FinanceManager.Domain.Enums;
using FinanceManager.Domain.Utilities;

public sealed record GetPagedBudgetResult(
    BudgetYearSummary? BudgetYear,
    IReadOnlyList<BudgetCell> Cells,
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
            var cells = BuildCells(budgetYear);
            var entries = (await budgetEntryRepository.GetByBudgetYearAsync(budgetYear.Id)).ToList();
            var totals = entries.Aggregate(new BudgetTotals(), (current, entry) => current.Add(entry));

            entries = mode switch
            {
                BudgetGridMode.Income => [.. entries.Where(entry => entry.Category.Group.IsIncome)],
                BudgetGridMode.Expense => [.. entries.Where(entry => !entry.Category.Group.IsIncome)],
                _ => entries
            };

            var poplulatedCells = await PopulateCells(cells, entries);

            var summary = BudgetYearSummary.FromBudgetYear(budgetYear);

            return new GetPagedBudgetResult(summary, poplulatedCells, categorySummaries, totals.Income - totals.Expense, totals.Income, totals.Expense);
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

    private static List<BudgetCell> BuildCells(BudgetYear budgetYear)
    {
        var cells = new List<BudgetCell>();

        var scopeStart = ScopeHelper.GetIsoYearStart(budgetYear.Scope, budgetYear.Year);
        var periodsCount = ScopeHelper.GetPeriodCount(budgetYear.Scope, budgetYear.Year);

        for (var i = 0; i < periodsCount; i++)
        {
            var cellStart = ScopeHelper.GetPeriodStart(budgetYear.Scope, scopeStart, i);
            cells.Add(new BudgetCell(i, cellStart, budgetYear.Scope));
        }

        return cells;
    }

    private async Task<List<BudgetCell>> PopulateCells(List<BudgetCell> cells, List<BudgetEntry> entries)
    {
        entries = [.. entries.OrderByDescending(e => e.Category.Group.IsIncome).ThenBy(e => e.Category.Group.Name).ThenBy(e => e.Category.Name)];

        for (var i = 0; i < entries.Count; i++)
        {
            for (var j = 0; j < entries[i].Length; j++)
            {
                var cell = cells[j + entries[i].ScopePosition];

                var query = new FilterQuery
                {
                    FilterDateFrom = cell.StartDate.ToDateTime(TimeOnly.MinValue),
                    FilterDateTo = ScopeHelper.GetPeriodEnd(cell.Scope ?? BudgetScope.Monthly, cell.StartDate).ToDateTime(TimeOnly.MaxValue),
                    FilterCategory = CategorySummary.FromCategory(entries[i].Category)
                };

                var realAmount = (await transactionRepository.GetTransactionsAsync(query)).Sum(t => t.Amount);

                cell.Entries.Add(BudgetCellEntry.FromBudgetEntry(entries[i], j, i, realAmount));
            }
        }

        return cells;
    }
}
