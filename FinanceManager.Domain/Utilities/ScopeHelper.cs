namespace FinanceManager.Domain.Utilities;

using FinanceManager.Domain.Constants;
using FinanceManager.Domain.Enums;
using System.Globalization;

public static class ScopeHelper
{
    public static int GetPeriodCountForYear(BudgetScope scope, int year)
    {
        return scope switch
        {
            BudgetScope.Monthly => DateConstants.MONTHS_IN_YEAR,
            BudgetScope.Weekly => ISOWeek.GetWeeksInYear(year),
            BudgetScope.Fortnightly => (int)Math.Ceiling((double)ISOWeek.GetWeeksInYear(year) / DateConstants.WEEKS_IN_FORTNIGHT),
            _ => throw new ArgumentOutOfRangeException(nameof(scope), "Invalid budget scope.")
        };
    }

    public static string GetScopeDescription(BudgetScope scope)
    {
        return scope switch
        {
            BudgetScope.Weekly => "week",
            BudgetScope.Fortnightly => "fortnight",
            BudgetScope.Monthly => "month",
            _ => throw new ArgumentOutOfRangeException(nameof(scope), "Invalid budget scope.")
        };
    }

    public static DateOnly GetPeriodStart(BudgetScope scope, DateOnly date, int offset = 0)
    {
        var periodStart = scope switch
        {
            BudgetScope.Weekly => GetWeekStart(date),
            BudgetScope.Fortnightly => GetFortnightStart(date),
            BudgetScope.Monthly => new DateOnly(date.Year, date.Month, 1),
            _ => throw new ArgumentOutOfRangeException(nameof(scope), "Invalid budget scope.")
        };

        return scope switch
        {
            BudgetScope.Weekly => periodStart.AddDays(DateConstants.DAYS_IN_WEEK * offset),
            BudgetScope.Fortnightly => periodStart.AddDays(DateConstants.DAYS_IN_FORTNIGHT * offset),
            BudgetScope.Monthly => periodStart.AddMonths(offset),
            _ => throw new ArgumentOutOfRangeException(nameof(scope), "Invalid budget scope.")
        };
    }

    public static DateOnly GetPeriodEnd(BudgetScope scope, DateOnly periodStart, int length = 1)
    {
        if (length < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(length), "Length must be at least 1.");
        }

        return scope switch
        {
            BudgetScope.Weekly => periodStart.AddDays(DateConstants.DAYS_IN_WEEK * length - 1),
            BudgetScope.Fortnightly => periodStart.AddDays(DateConstants.DAYS_IN_FORTNIGHT * length - 1),
            BudgetScope.Monthly => periodStart.AddMonths(length).AddDays(-1),
            _ => throw new ArgumentOutOfRangeException(nameof(scope), "Invalid budget scope.")
        };
    }

    public static DateOnly GetYearStart(BudgetScope scope, int year)
    {
        return scope switch
        {
            BudgetScope.Weekly => DateOnly.FromDateTime(ISOWeek.ToDateTime(year, 1, DayOfWeek.Monday)),
            BudgetScope.Fortnightly => DateOnly.FromDateTime(ISOWeek.ToDateTime(year, 1, DayOfWeek.Monday)),
            BudgetScope.Monthly => new DateOnly(year, 1, 1),
            _ => throw new ArgumentOutOfRangeException(nameof(scope), "Invalid budget scope.")
        };
    }

    private static DateOnly GetWeekStart(DateOnly date)
    {
        var diff = (DateConstants.DAYS_IN_WEEK + (date.DayOfWeek - DayOfWeek.Monday)) % DateConstants.DAYS_IN_WEEK;
        return date.AddDays(-diff);
    }

    private static DateOnly GetFortnightStart(DateOnly date)
    {
        var dt = date.ToDateTime(TimeOnly.MinValue);
        var isoYear = ISOWeek.GetYear(dt);
        var isoWeek = ISOWeek.GetWeekOfYear(dt);
        var fortnightStartWeek = (isoWeek - 1) / DateConstants.WEEKS_IN_FORTNIGHT * DateConstants.WEEKS_IN_FORTNIGHT + 1;

        var start = ISOWeek.ToDateTime(isoYear, fortnightStartWeek, DayOfWeek.Monday);
        return DateOnly.FromDateTime(start);
    }
}