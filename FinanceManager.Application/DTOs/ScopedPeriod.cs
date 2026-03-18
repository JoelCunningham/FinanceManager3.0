namespace FinanceManager.Application.DTOs;

using FinanceManager.Domain.Enums;
using System.Globalization;

public record ScopedPeriod : InnerPeriod
{
    public int Length { get; init; }
    public BudgetScope Scope { get; init; }
    public IEnumerable<InnerPeriod> InnerPeriods { get; init; }
    public string ScopeDescription { get; init; }

    public ScopedPeriod(BudgetScope scope, DateOnly containingDate, int length = 1)
    {
        if (length < 1) throw new ArgumentOutOfRangeException(nameof(length), "Length must be at least 1.");
        
        Length = length;

        Scope = scope;
        ScopeDescription = scope switch
        {
            BudgetScope.Weekly => "week",
            BudgetScope.Fortnightly => "fortnight",
            BudgetScope.Monthly => "month",
            _ => throw new ArgumentOutOfRangeException(nameof(scope), "Invalid budget scope.")
        };

        StartDate = scope switch
        {
            BudgetScope.Weekly => GetWeekStart(containingDate),
            BudgetScope.Fortnightly => GetFortnightStart(containingDate),
            BudgetScope.Monthly => GetMonthStart(containingDate),
            _ => throw new ArgumentOutOfRangeException(nameof(scope), "Invalid budget scope.")
        };
        EndDate = scope switch
        {
            BudgetScope.Weekly => StartDate.AddDays(7 * length - 1),
            BudgetScope.Fortnightly => StartDate.AddDays(14 * length - 1),
            BudgetScope.Monthly => StartDate.AddMonths(length).AddDays(-1),
            _ => throw new ArgumentOutOfRangeException(nameof(scope), "Invalid budget scope.")
        };

        InnerPeriods = GetInnerPeriods();
    }

    private List<InnerPeriod> GetInnerPeriods()
    {
        var innerPeriods = new List<InnerPeriod>();
        var currentStart = StartDate;
        for (int i = 0; i < Length; i++)
        {
            var currentEnd = Scope switch
            {
                BudgetScope.Weekly => currentStart.AddDays(6),
                BudgetScope.Fortnightly => currentStart.AddDays(13),
                BudgetScope.Monthly => currentStart.AddMonths(1).AddDays(-1),
                _ => throw new ArgumentOutOfRangeException(nameof(Scope), "Invalid budget scope.")
            };
            innerPeriods.Add(new InnerPeriod { StartDate = currentStart, EndDate = currentEnd });
            currentStart = currentEnd.AddDays(1);
        }
        return innerPeriods;
    }

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

public record InnerPeriod
{
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }

    public bool IsInPeriod(DateOnly date)
    {
        return date >= StartDate && date <= EndDate;
    }
}