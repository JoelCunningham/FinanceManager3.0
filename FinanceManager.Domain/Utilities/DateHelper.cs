namespace FinanceManager.Domain.Utilities;

using FinanceManager.Domain.Constants;
using FinanceManager.Domain.Enums;
using System.Globalization;

public class DateHelper
{
    public static DateOnly GetIsoWeek1(int year) => DateOnly.FromDateTime(ISOWeek.GetYearStart(year));

    public static int GetWeekIndex(DateOnly date) => ISOWeek.GetWeekOfYear(date.ToDateTime(TimeOnly.MinValue));
    public static int GetFortnightIndex(DateOnly date) => ((GetWeekIndex(date) - 1) / DateConstants.WEEKS_IN_FORTNIGHT) + 1;
    public static int GetMonthIndex(DateOnly date) => date.Month;

    public static int GetPeriodIndex(DateOnly date, BudgetScope scope) => scope switch
    {
        BudgetScope.Weekly => GetWeekIndex(date),
        BudgetScope.Fortnightly => GetFortnightIndex(date),
        BudgetScope.Monthly => GetMonthIndex(date),
        _ => throw new ArgumentOutOfRangeException(nameof(scope), "Invalid budget scope.")
    };

    public static int GetWeekCount(int year) => ISOWeek.GetWeeksInYear(year);
    public static int GetFortnightCount(int year) => (int)Math.Ceiling((double)GetWeekCount(year) / DateConstants.WEEKS_IN_FORTNIGHT);
    public static int GetMonthCount() => DateConstants.MONTHS_IN_YEAR;

    public static int GetPeriodCount(int year, BudgetScope scope) => scope switch
    {
        BudgetScope.Weekly => GetWeekCount(year),
        BudgetScope.Fortnightly => GetFortnightCount(year),
        BudgetScope.Monthly => GetMonthCount(),
        _ => throw new ArgumentOutOfRangeException(nameof(scope), "Invalid budget scope.")
    };

    public static DateOnly GetWeekStart(DateOnly date)
    {
        var diff = (DateConstants.DAYS_IN_WEEK + (date.DayOfWeek - DayOfWeek.Monday)) % DateConstants.DAYS_IN_WEEK;
        return date.AddDays(-diff);
    }

    public static DateOnly GetFortnightStart(DateOnly date)
    {
        var isoYear = ISOWeek.GetYear(date.ToDateTime(TimeOnly.MinValue));
        var fortnightStartWeek = (GetWeekIndex(date) - 1) / DateConstants.WEEKS_IN_FORTNIGHT * DateConstants.WEEKS_IN_FORTNIGHT + 1;

        var start = ISOWeek.ToDateTime(isoYear, fortnightStartWeek, DayOfWeek.Monday);
        return DateOnly.FromDateTime(start);
    }

    public static DateOnly GetMonthStart(DateOnly date) => new(date.Year, date.Month, 1);
}
