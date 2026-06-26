namespace FinanceManager.Domain.Utilities;

using FinanceManager.Domain.Entities;
using FinanceManager.Domain.Enums;

public static class BudgetYearHelper
{
    public static IEnumerable<BudgetEntryPeriod> GetPeriods(BudgetEntry entry)
    {
        for (var i = 0; i < entry.Length; i++)
        {
            var periodStart = ScopeHelper.GetPeriodStart(entry.BudgetYear.Scope, entry.BudgetYear.StartDate, entry.ScopePosition + i);
            var periodEnd = ScopeHelper.GetPeriodEnd(entry.BudgetYear.Scope, periodStart);

            var daysInPeriod = periodEnd.DayNumber - periodStart.DayNumber + 1;
            yield return new BudgetEntryPeriod(periodStart, periodEnd, entry.Amount / daysInPeriod);
        }
    }

    public static IEnumerable<BudgetEntryPeriod> GetPeriods(int year, BudgetScope scope)
    {
        return Enumerable.Range(0, ScopeHelper.GetPeriodCount(scope, year))
            .Select(i =>
            {
                var periodStart = ScopeHelper.GetPeriodStart(scope, new DateOnly(year, 1, 1), i);
                var periodEnd = ScopeHelper.GetPeriodEnd(scope, periodStart);
                return new BudgetEntryPeriod(periodStart, periodEnd, 0m);
            });
    }

    public static IEnumerable<BudgetEntryDayAmount> GetOverlappingDays(BudgetEntry entry, DateOnly rangeStart, DateOnly rangeEnd)
    {
        foreach (var period in GetPeriods(entry))
        {
            var overlapStart = period.StartDate > rangeStart ? period.StartDate : rangeStart;
            var overlapEnd = period.EndDate < rangeEnd ? period.EndDate : rangeEnd;

            if (overlapStart > overlapEnd)
            {
                continue;
            }

            var day = overlapStart;
            while (day <= overlapEnd)
            {
                yield return new BudgetEntryDayAmount(day, period.DailyAmount);
                day = day.AddDays(1);
            }
        }
    }

    public static bool HasAnyPeriodOverlap(BudgetEntry entry, DateOnly rangeStart, DateOnly rangeEnd)
    {
        return GetPeriods(entry).Any(period => period.EndDate >= rangeStart && period.StartDate <= rangeEnd);
    }
}

public readonly record struct BudgetEntryPeriod(DateOnly StartDate, DateOnly EndDate, decimal DailyAmount);
public readonly record struct BudgetEntryDayAmount(DateOnly Date, decimal DailyAmount);
