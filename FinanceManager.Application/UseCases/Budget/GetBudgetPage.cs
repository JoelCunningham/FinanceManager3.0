namespace FinanceManager.Application.UseCases.Budget;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Interfaces;
using FinanceManager.Application.Utilities;
using FinanceManager.Domain.Constants;
using FinanceManager.Domain.Entities;
using FinanceManager.Domain.Enums;

public sealed record GetBudgetPageResult(
    BudgetPeriod? BudgetPeriod,
    IReadOnlyList<BudgetCell> Cells,
    IReadOnlyList<CategorySummary> Categories
);

public sealed class GetBudgetPage(IBudgetEntryRepository budgetEntryRepository, ICategoryRepository categoryRepository, IBudgetPeriodRepository budgetPeriodRepository)
{
    public async Task<GetBudgetPageResult> ExecuteAsync(int year)
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
            var poplulatedCells = PopulateCells(cells, entries);       

            return new GetBudgetPageResult(period, poplulatedCells, categorySummaries);
        }
    }

    private static List<BudgetCell> BuildCells(BudgetPeriod period)
    {
        var cells = new List<BudgetCell>();

        if (period.Scope == BudgetScope.Monthly)
        {
            var monthsInYear = DateHelper.GetMonthCount();
            for (var i = 0; i < monthsInYear; i++)
            {
                cells.Add(new BudgetCell(i, period.StartDate.AddMonths(i), BudgetScope.Monthly));
            }
        }

        var firstWeekOfYear = DateHelper.GetIsoWeek1(period.Year);

        if (period.Scope == BudgetScope.Weekly)
        {
            var weeksInYear = DateHelper.GetWeekCount(period.Year);
            for (var i = 0; i < weeksInYear; i++)
            {
                cells.Add(new BudgetCell(i, firstWeekOfYear.AddDays(i * DateConstants.DAYS_IN_WEEK), BudgetScope.Weekly));
            }
        }

        if (period.Scope == BudgetScope.Fortnightly)
        {
            var fortnightsInYear = DateHelper.GetFortnightCount(period.Year);
            for (var i = 0; i < fortnightsInYear; i++)
            {
                cells.Add(new BudgetCell(i, firstWeekOfYear.AddDays(i * DateConstants.DAYS_IN_FORTNIGHT), BudgetScope.Fortnightly));
            }
        }

        return cells;
    }

    private static List<BudgetCell> PopulateCells(List<BudgetCell> cells, IReadOnlyList<BudgetEntry> entries)
    {
        foreach (var entry in entries)
        {
            for (var i = 0; i < entry.Length; i++)
            {
                cells[i + entry.PeriodPosition].Entries.Add(BudgetCellEntry.FromBudgetEntry(entry, i));
            }
        }

        foreach (var cell in cells)
        {
            cell.Entries = [.. cell.Entries
                .OrderByDescending(e => e.Category?.IsIncome == true)
                .ThenBy(e => e.Category?.GroupName)
                .ThenBy(e => e.Category?.Name)];
        }

        return cells;
    }
}
