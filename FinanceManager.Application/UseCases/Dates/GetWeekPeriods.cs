namespace FinanceManager.Application.UseCases.Dates;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Utilities;
using FinanceManager.Domain.Constants;
using FinanceManager.Domain.Enums;

public sealed record GetWeekPeriodsResult(IEnumerable<ScopedPeriod> Periods) : UseCaseResult;

public sealed class GetWeekPeriods
{
    public static GetWeekPeriodsResult Execute(int year)
    {
        var isoWeek1 = DateHelper.GetIsoWeek1(year);
        var weeksInYear = DateHelper.GetWeekCount(year);

        var periods = new List<ScopedPeriod>();

        for (int weekNo = 1; weekNo <= weeksInYear; weekNo++)
        {
            var periodDate = isoWeek1.AddDays(DateConstants.DAYS_IN_WEEK * (weekNo - 1));
            periods.Add(new ScopedPeriod(BudgetScope.Weekly, periodDate));
        }

        return new GetWeekPeriodsResult(periods);
    }
}
