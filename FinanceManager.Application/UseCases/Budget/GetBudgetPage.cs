namespace FinanceManager.Application.UseCases.Budget;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Enums;
using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Entities;
using FinanceManager.Domain.Utilities;

public sealed record GetBudgetPageResult(
    BudgetPeriod? BudgetPeriod,
    IReadOnlyList<BudgetCell> Cells,
    IReadOnlyList<CategorySummary> Categories
);

public sealed class GetBudgetPage(IBudgetEntryRepository budgetEntryRepository, ICategoryRepository categoryRepository, IBudgetPeriodRepository budgetPeriodRepository)
{
    public async Task<GetBudgetPageResult> ExecuteAsync(int year, BudgetGridMode mode = BudgetGridMode.Net)
    {
        var period = await budgetPeriodRepository.GetByYearAsync(year);
        var categories = await categoryRepository.GetAllAsync();

        var categorySummaries = categories.Select(CategorySummary.FromCategory).ToList();

        if (period == null)
        {
            return new GetBudgetPageResult(null, [], categorySummaries);
        }
        else
        {
            var cells = BuildCells(period);
            var entries = (await budgetEntryRepository.GetByPeriodAsync(period.Id)).ToList();

            entries = mode switch
            {
                BudgetGridMode.Income => [.. entries.Where(entry => entry.Category.Group.IsIncome)],
                BudgetGridMode.Expense => [.. entries.Where(entry => !entry.Category.Group.IsIncome)],
                _ => entries
            };

            var poplulatedCells = PopulateCells(cells, entries);

            return new GetBudgetPageResult(period, poplulatedCells, categorySummaries);
        }
    }

    private static List<BudgetCell> BuildCells(BudgetPeriod period)
    {
        var cells = new List<BudgetCell>();

        var scopeStart = ScopeHelper.GetYearStart(period.Scope, period.Year);
        var periodsCount = ScopeHelper.GetPeriodCountForYear(period.Scope, period.Year);

        for (var i = 0; i < periodsCount; i++)
        {
            var cellStart = ScopeHelper.GetPeriodStart(period.Scope, scopeStart, i);
            cells.Add(new BudgetCell(i, cellStart, period.Scope));
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
                cells[j + entries[i].PeriodPosition].Entries.Add(BudgetCellEntry.FromBudgetEntry(entries[i], j, i));
            }
        }

        return cells;
    }
}
