namespace FinanceManager.Application.UseCases.Dates;

using FinanceManager.Application.DTOs;
using FinanceManager.Domain.Constants;
using FinanceManager.Domain.Enums;
using FinanceManager.Domain.Utilities;

public sealed record GetWeekRangesResult(IEnumerable<ScopedRange> Ranges) : UseCaseResult;

public sealed class GetWeekRanges
{
    public static GetWeekRangesResult Execute(int year)
    {
        var isoWeek1 = DateHelper.GetIsoWeek1(year);
        var weeksInYear = DateHelper.GetWeekCount(year);

        var ranges = new List<ScopedRange>();

        for (int weekNo = 1; weekNo <= weeksInYear; weekNo++)
        {
            var rangeDate = isoWeek1.AddDays(DateConstants.DAYS_IN_WEEK * (weekNo - 1));
            ranges.Add(new ScopedRange(BudgetScope.Weekly, rangeDate));
        }

        return new GetWeekRangesResult(ranges);
    }
}
