namespace FinanceManager.Domain.Utilities;

using FinanceManager.Domain.Constants;
using FinanceManager.Domain.Enums;
using System.Globalization;

public static class ScopeHelper
{
    public static int GetPeriodCount(BudgetScope scope, int year)
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

    public static DateOnly GetRangeStart(BudgetScope scope, DateOnly date)
    {
       return GetPeriodStart(scope, date, 0);
    }

    public static DateOnly GetPeriodStart(BudgetScope scope, DateOnly date, int offset)
    {
        return scope switch
        {
            BudgetScope.Weekly => DateHelper.GetWeekStart(date).AddDays(DateConstants.DAYS_IN_WEEK * offset),
            BudgetScope.Fortnightly => DateHelper.GetFortnightStart(date).AddDays(DateConstants.DAYS_IN_FORTNIGHT * offset),
            BudgetScope.Monthly => DateHelper.GetMonthStart(date).AddMonths(offset),
            _ => throw new ArgumentOutOfRangeException(nameof(scope), "Invalid budget scope.")
        };
    }

    public static DateOnly GetPeriodEnd(BudgetScope scope, DateOnly periodStart)
    {
        return GetRangeEnd(scope, periodStart, 1);
    }

    public static DateOnly GetRangeEnd(BudgetScope scope, DateOnly rangeStart, int length)
    {
        if (length < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(length), "Length must be at least 1.");
        }

        return scope switch
        {
            BudgetScope.Weekly => rangeStart.AddDays(DateConstants.DAYS_IN_WEEK * length - 1),
            BudgetScope.Fortnightly => rangeStart.AddDays(DateConstants.DAYS_IN_FORTNIGHT * length - 1),
            BudgetScope.Monthly => rangeStart.AddMonths(length).AddDays(-1),
            _ => throw new ArgumentOutOfRangeException(nameof(scope), "Invalid budget scope.")
        };
    }

    public static DateOnly GetIsoYearStart(BudgetScope scope, int year)
    {
        return scope switch
        {
            BudgetScope.Weekly => DateOnly.FromDateTime(ISOWeek.ToDateTime(year, 1, DayOfWeek.Monday)),
            BudgetScope.Fortnightly => DateOnly.FromDateTime(ISOWeek.ToDateTime(year, 1, DayOfWeek.Monday)),
            BudgetScope.Monthly => new DateOnly(year, 1, 1),
            _ => throw new ArgumentOutOfRangeException(nameof(scope), "Invalid budget scope.")
        };
    }
}