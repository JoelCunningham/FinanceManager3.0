namespace FinanceManager.Application.Services;

using FinanceManager.Application.DTOs;
using FinanceManager.Domain.Constants;
using FinanceManager.Domain.Enums;
using FinanceManager.Domain.Helpers;
using System.Globalization;

public class DateService()
{
    public static IEnumerable<ScopedPeriod> GetWeeks(int year)
    {
        var isoWeek1 = DateHelper.GetIsoWeek1(year);
        var weeksInYear = DateHelper.GetWeekCount(year);

        for (int weekNo = 1; weekNo <= weeksInYear; weekNo++)
        {
            var periodDate = isoWeek1.AddDays(DateConstants.DAYS_IN_WEEK * (weekNo - 1));
            yield return new ScopedPeriod(BudgetScope.Weekly, periodDate);
        }
    }

    public static IEnumerable<ScopedPeriod> GetFortnights(int year)
    {
        var weeksInYear = DateHelper.GetWeekCount(year);

        for (int startIsoWeek = 1; startIsoWeek <= weeksInYear; startIsoWeek += DateConstants.WEEKS_IN_FORTNIGHT)
        {
            var startDt = ISOWeek.ToDateTime(year, startIsoWeek, DayOfWeek.Monday);
            var startDate = DateOnly.FromDateTime(startDt);

            yield return new ScopedPeriod(BudgetScope.Fortnightly, startDate);
        }
    }

    public static IEnumerable<ScopedPeriod> GetMonths(int year)
    {
        for (int monthNo = 1; monthNo <= DateConstants.MONTHS_IN_YEAR; monthNo++)
        {
            var periodDate = new DateOnly(year, monthNo, 1);
            yield return new ScopedPeriod(BudgetScope.Monthly, periodDate);
        }
    }
}