namespace FinanceManager.Application.UseCases.Budget;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Constants;
using FinanceManager.Domain.Entities;
using FinanceManager.Domain.Enums;

public sealed record GetBudgetPageResult(
    IReadOnlyList<BudgetCell> Cells,
    IReadOnlyList<CategorySummary> Categories
);

public sealed class GetBudgetPage(IBudgetEntryRepository budgetEntryRepository, ICategoryRepository categoryRepository)
{
    public async Task<GetBudgetPageResult> ExecuteAsync(ScopedPeriod period)
    {
        var categories = (await categoryRepository.GetAllAsync()).ToList();
        var categoryIds = categories.Select(c => c.Id).ToArray();
        var categorySummaries = categories.Select(CategorySummary.FromCategory).ToList();

        var entries = (await budgetEntryRepository.GetByRangeAsync(period.StartDate, period.EndDate, categoryIds)).ToList();

        var cells = BuildCells(period);
        PopulateCells(period, cells, entries);

        return new GetBudgetPageResult(cells, categorySummaries);
    }

    public static int GetCellIndexForEntry(ScopedPeriod period, BudgetEntry entry)
    {
        var innerPeriods = period.InnerPeriods.ToList();
        var entryPeriod = innerPeriods.FirstOrDefault(p => entry.StartDate >= p.StartDate && entry.StartDate <= p.EndDate);
        return entryPeriod is not null ? innerPeriods.IndexOf(entryPeriod) : -1;
    }

    private static List<BudgetCell> BuildCells(ScopedPeriod period)
    {
        var cells = new List<BudgetCell>();

        if (period.Scope == BudgetScope.Monthly)
        {
            for (var i = 0; i < DateConstants.MONTHS_IN_YEAR; i++)
            {
                var dt = period.StartDate.AddMonths(i);
                cells.Add(new BudgetCell(i, dt.ToString("MMM")));
            }
        }
        else if (period.Scope == BudgetScope.Weekly)
        {
            for (var i = 0; i < DateConstants.DAYS_IN_WEEK; i++)
            {
                var dt = period.StartDate.AddDays(i);
                cells.Add(new BudgetCell(i, dt.ToString("ddd dd")));
            }
        }
        else
        {
            for (var i = 0; i < DateConstants.DAYS_IN_FORTNIGHT; i++)
            {
                var dt = period.StartDate.AddDays(i);
                cells.Add(new BudgetCell(i, dt.ToString("ddd dd")));
            }
        }

        return cells;
    }

    private static void PopulateCells(ScopedPeriod period, List<BudgetCell> cells, IReadOnlyList<BudgetEntry> entries)
    {
        foreach (var cell in cells)
        {
            cell.Entries.Clear();
            cell.ScopeType = CellScopeType.None;
            cell.Scope = null;
        }

        foreach (var entry in entries)
        {
            var cellIndex = GetCellIndexForEntry(period, entry);
            if (cellIndex < 0) continue;

            cells[cellIndex].Entries.Add(entry);
            UpdateCellScope(period, cells, entry, cellIndex);
        }

        foreach (var cell in cells)
        {
            cell.Entries = [.. cell.Entries
                .OrderByDescending(e => e.Category.Group?.IsIncome == true)
                .ThenBy(e => e.Category.Group.Name)
                .ThenBy(e => e.Category.Name)];
        }
    }

    private static void UpdateCellScope(ScopedPeriod period, List<BudgetCell> cells, BudgetEntry entry, int cellIndex)
    {
        var entryScope = entry.Period.Scope == period.Scope
            ? CellScopeType.Current
            : CellScopeType.Other;

        var currentScope = cells[cellIndex].ScopeType;

        if (currentScope == CellScopeType.None)
        {
            cells[cellIndex].ScopeType = entryScope;
            cells[cellIndex].Scope = entry.Period.Scope;
        }
        else if (currentScope != entryScope)
        {
            cells[cellIndex].ScopeType = CellScopeType.Mixed;
            cells[cellIndex].Scope = null;
        }
    }
}
