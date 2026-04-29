namespace FinanceManager.Application.UseCases.Budget;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Enums;
using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Entities;
using FinanceManager.Domain.Utilities;

public sealed record GetPagedBudgetResult(
    BudgetYear? BudgetYear,
    IReadOnlyList<BudgetCell> Cells,
    IReadOnlyList<CategorySummary> Categories
);

public sealed class GetPagedBudget(IBudgetEntryRepository budgetEntryRepository, ICategoryRepository categoryRepository, IBudgetYearRepository budgetYearRepository)
{
    public async Task<GetPagedBudgetResult> ExecuteAsync(int year, BudgetGridMode mode = BudgetGridMode.Net)
    {
        var budgetYear = await budgetYearRepository.GetByYearAsync(year);
        var categories = await categoryRepository.GetAllAsync();

        var categorySummaries = categories.Select(CategorySummary.FromCategory).ToList();

        if (budgetYear == null)
        {
            return new GetPagedBudgetResult(null, [], categorySummaries);
        }
        else
        {
            var cells = BuildCells(budgetYear);
            var entries = (await budgetEntryRepository.GetByBudgetYearAsync(budgetYear.Id)).ToList();

            entries = mode switch
            {
                BudgetGridMode.Income => [.. entries.Where(entry => entry.Category.Group.IsIncome)],
                BudgetGridMode.Expense => [.. entries.Where(entry => !entry.Category.Group.IsIncome)],
                _ => entries
            };

            var poplulatedCells = PopulateCells(cells, entries);

            return new GetPagedBudgetResult(budgetYear, poplulatedCells, categorySummaries);
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

    private static List<BudgetCell> PopulateCells(List<BudgetCell> cells, List<BudgetEntry> entries)
    {
        entries = [.. entries
            .OrderByDescending(e => e.Length)
            .ThenByDescending(e => e.Category.Group.IsIncome)
            .ThenBy(e => e.Category.Group.Name)
            .ThenBy(e => e.Category.Name)];

        for (var i = 0; i < entries.Count; i++)
        {
            for (var j = 0; j < entries[i].Length; j++)
            {
                cells[j + entries[i].ScopePosition].Entries.Add(BudgetCellEntry.FromBudgetEntry(entries[i], j, i));
            }
        }

        return cells;
    }
}
