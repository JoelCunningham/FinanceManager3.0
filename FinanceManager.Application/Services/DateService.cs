namespace FinanceManager.Application.Services;

using FinanceManager.Application.DTOs;
using FinanceManager.Domain.Enums;
using System.Globalization;

public class DateService()
{
    private static DateOnly GetIsoWeek1(int year) => new(year, 1, ISO_WEEK1_DAY_OF_YEAR);

    public static int GetWeekCount(int year) => ISOWeek.GetWeeksInYear(year);
    public static int GetFortnightCount(int year) => (GetWeekCount(year) + 1) / WEEKS_IN_FORTNIGHT;
    public static int GetMonthCount() => MONTHS_IN_YEAR;

    public static IEnumerable<BudgetPeriod> GetWeeks(int year)
    {
        var isoWeek1 = GetIsoWeek1(year);
        var weeksInYear = GetWeekCount(year);

        for (int weekNo = 1; weekNo <= weeksInYear; weekNo++)
        {
            var periodDate = isoWeek1.AddDays(DAYS_IN_WEEK * (weekNo - 1));
            yield return new BudgetPeriod(Scope.Weekly, periodDate);
        }
    }

    public static IEnumerable<BudgetPeriod> GetFortnights(int year)
    {
        var weeksInYear = GetWeekCount(year);

        for (int startIsoWeek = 1; startIsoWeek <= weeksInYear; startIsoWeek += WEEKS_IN_FORTNIGHT)
        {
            var startDt = ISOWeek.ToDateTime(year, startIsoWeek, DayOfWeek.Monday);
            var startDate = DateOnly.FromDateTime(startDt);

            yield return new BudgetPeriod(Scope.Fortnightly, startDate);
        }
    }

    public static IEnumerable<BudgetPeriod> GetMonths(int year)
    {
        for (int monthNo = 1; monthNo <= MONTHS_IN_YEAR; monthNo++)
        {
            var periodDate = new DateOnly(year, monthNo, 1);
            yield return new BudgetPeriod(Scope.Monthly, periodDate);
        }
    }

    public static int GetWeekIndex(DateOnly date) => ISOWeek.GetWeekOfYear(date.ToDateTime(TimeOnly.MinValue));

    public static int GetFortnightIndex(DateOnly date) => ((GetWeekIndex(date) - 1) / WEEKS_IN_FORTNIGHT) + 1;

    public static int GetMonthIndex(DateOnly date) => date.Month;

    #region Constants
    private static readonly int DAYS_IN_WEEK = 7;
    private static readonly int WEEKS_IN_FORTNIGHT = 2;
    private static readonly int MONTHS_IN_YEAR = 12;
    private static readonly int ISO_WEEK1_DAY_OF_YEAR = 4;
    #endregion
}