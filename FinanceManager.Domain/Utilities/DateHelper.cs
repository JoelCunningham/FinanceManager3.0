using System.Globalization;
using FinanceManager.Domain.Constants;

namespace FinanceManager.Application.Utilities;

public class DateHelper
{
    public static DateOnly GetIsoWeek1(int year) => DateOnly.FromDateTime(ISOWeek.GetYearStart(year));

    public static int GetWeekIndex(DateOnly date) => ISOWeek.GetWeekOfYear(date.ToDateTime(TimeOnly.MinValue));
    public static int GetFortnightIndex(DateOnly date) => ((GetWeekIndex(date) - 1) / DateConstants.WEEKS_IN_FORTNIGHT) + 1;
    public static int GetMonthIndex(DateOnly date) => date.Month;

    public static int GetWeekCount(int year) => ISOWeek.GetWeeksInYear(year);
    public static int GetFortnightCount(int year) => (int)Math.Ceiling((double)GetWeekCount(year) / DateConstants.WEEKS_IN_FORTNIGHT);
    public static int GetMonthCount() => DateConstants.MONTHS_IN_YEAR;
}
