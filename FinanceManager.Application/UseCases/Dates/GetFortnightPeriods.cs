namespace FinanceManager.Application.UseCases.Dates;

using FinanceManager.Application.DTOs;
using FinanceManager.Domain.Constants;
using FinanceManager.Domain.Enums;
using System.Globalization;

public sealed record GetFortnightPeriodsResult(IEnumerable<ScopedPeriod> Periods) : UseCaseResult;

public sealed class GetFortnightPeriods
{
    public static GetFortnightPeriodsResult Execute(int year)
    {
        var weeksInYear = ISOWeek.GetWeeksInYear(year);
        var periods = new List<ScopedPeriod>();

        for (int startIsoWeek = 1; startIsoWeek <= weeksInYear; startIsoWeek += DateConstants.WEEKS_IN_FORTNIGHT)
        {
            var startDt = ISOWeek.ToDateTime(year, startIsoWeek, DayOfWeek.Monday);
            var startDate = DateOnly.FromDateTime(startDt);

            periods.Add(new ScopedPeriod(BudgetScope.Fortnightly, startDate));
        }

        return new GetFortnightPeriodsResult(periods);
    }
}
