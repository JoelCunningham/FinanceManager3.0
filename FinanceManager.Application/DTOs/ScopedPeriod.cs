namespace FinanceManager.Application.DTOs;

using FinanceManager.Domain.Enums;
using System.Globalization;

public record ScopedPeriod
{
    public ScopedPeriod(Scope scope, DateOnly containingDate, int length = 1)
    {
        if (length < 1) throw new ArgumentOutOfRangeException(nameof(length), "Length must be at least 1.");

        StartDate = scope switch
        {
            Scope.Weekly => GetWeekStart(containingDate),
            Scope.Fortnightly => GetFortnightStart(containingDate),
            Scope.Monthly => GetMonthStart(containingDate),
            _ => throw new ArgumentOutOfRangeException(nameof(scope), "Invalid budget scope.")
        };
        EndDate = scope switch
        {
            Scope.Weekly => StartDate.AddDays(7 * length - 1),
            Scope.Fortnightly => StartDate.AddDays(14 * length - 1),
            Scope.Monthly => StartDate.AddMonths(length).AddDays(-1),
            _ => throw new ArgumentOutOfRangeException(nameof(scope), "Invalid budget scope.")
        };
    }

    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }

    private static DateOnly GetWeekStart(DateOnly date)
    {
        int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.AddDays(-diff);
    }

    private static DateOnly GetFortnightStart(DateOnly date)
    {
        var dt = date.ToDateTime(TimeOnly.MinValue);

        var isoYear = ISOWeek.GetYear(dt);
        var isoWeek = ISOWeek.GetWeekOfYear(dt);

        var fortnightStartWeek = ((isoWeek - 1) / 2) * 2 + 1;

        var start = ISOWeek.ToDateTime(isoYear, fortnightStartWeek, DayOfWeek.Monday);
        return DateOnly.FromDateTime(start);
    }

    private static DateOnly GetMonthStart(DateOnly date) => new(date.Year, date.Month, 1);
}