namespace FinanceManager.Domain.Utilities;

using FinanceManager.Domain.Entities;

public static class BudgetEntryPeriodHelper
{
    public static IEnumerable<BudgetEntrySegment> GetSegments(BudgetEntry entry)
    {
        for (var i = 0; i < entry.Length; i++)
        {
            var segmentStart = ScopeHelper.GetPeriodStart(entry.Period.Scope, entry.Period.StartDate, entry.PeriodPosition + i);
            var segmentEnd = ScopeHelper.GetPeriodEnd(entry.Period.Scope, segmentStart);

            var daysInSegment = segmentEnd.DayNumber - segmentStart.DayNumber + 1;
            yield return new BudgetEntrySegment(segmentStart, segmentEnd, entry.Amount / daysInSegment);
        }
    }

    public static IEnumerable<BudgetEntryDayAmount> GetOverlappingDays(BudgetEntry entry, DateOnly rangeStart, DateOnly rangeEnd)
    {
        foreach (var segment in GetSegments(entry))
        {
            var overlapStart = segment.StartDate > rangeStart ? segment.StartDate : rangeStart;
            var overlapEnd = segment.EndDate < rangeEnd ? segment.EndDate : rangeEnd;

            if (overlapStart > overlapEnd)
            {
                continue;
            }

            var day = overlapStart;
            while (day <= overlapEnd)
            {
                yield return new BudgetEntryDayAmount(day, segment.DailyAmount);
                day = day.AddDays(1);
            }
        }
    }

    public static bool HasAnySegmentOverlap(BudgetEntry entry, DateOnly rangeStart, DateOnly rangeEnd)
    {
        return GetSegments(entry).Any(segment => segment.EndDate >= rangeStart && segment.StartDate <= rangeEnd);
    }
}

public readonly record struct BudgetEntrySegment(DateOnly StartDate, DateOnly EndDate, decimal DailyAmount);
public readonly record struct BudgetEntryDayAmount(DateOnly Date, decimal DailyAmount);
