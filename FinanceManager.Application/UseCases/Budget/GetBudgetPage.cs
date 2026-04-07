namespace FinanceManager.Application.UseCases.Budget;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Enums;
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
