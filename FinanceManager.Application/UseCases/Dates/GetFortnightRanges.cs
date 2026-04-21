namespace FinanceManager.Application.UseCases.Dates;

using FinanceManager.Application.DTOs;
using FinanceManager.Domain.Constants;
using FinanceManager.Domain.Enums;
using System.Globalization;

public sealed record GetFortnightRangesResult(IEnumerable<ScopedRange> Ranges) : UseCaseResult;

public sealed class GetFortnightRanges
{
    public static GetFortnightRangesResult Execute(int year)
    {
        var weeksInYear = ISOWeek.GetWeeksInYear(year);
        var ranges = new List<ScopedRange>();

        for (int startIsoWeek = 1; startIsoWeek <= weeksInYear; startIsoWeek += DateConstants.WEEKS_IN_FORTNIGHT)
        {
            var startDate = ISOWeek.ToDateOnly(year, startIsoWeek, DayOfWeek.Monday);
            ranges.Add(new ScopedRange(BudgetScope.Fortnightly, startDate));
        }

        return new GetFortnightRangesResult(ranges);
    }
}
