namespace FinanceManager.Application.DTOs;

using FinanceManager.Domain.Enums;
using FinanceManager.Domain.Utilities;

public record ScopedRange : ScopedPeriod
{
    public int Length { get; init; }
    public IEnumerable<ScopedPeriod> Periods { get; init; }

    public ScopedRange(BudgetScope scope, DateOnly containingDate, int length = 1)
    {
        if (length < 1) throw new ArgumentOutOfRangeException(nameof(length), "Length must be at least 1.");

        Scope = scope;
        StartDate = ScopeHelper.GetRangeStart(scope, containingDate);
        EndDate = ScopeHelper.GetRangeEnd(scope, StartDate, length);

        Length = length;
        Periods = GetPeriodsInRange();
    }

    private IEnumerable<ScopedPeriod> GetPeriodsInRange()
    {
        for (int i = 0; i < Length; i++)
        {
            yield return new ScopedPeriod(Scope, StartDate, i);
        }
    }
}

public record ScopedPeriod
{
    public BudgetScope Scope { get; init; }
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
    public string ScopeDescription => ScopeHelper.GetScopeDescription(Scope);

    public ScopedPeriod() { }

    public ScopedPeriod(BudgetScope scope, DateOnly containingDate, int offset)
    {
        Scope = scope;
        StartDate = ScopeHelper.GetPeriodStart(scope, containingDate, offset);
        EndDate = ScopeHelper.GetPeriodEnd(scope, StartDate);
    }

    public bool Includes(DateOnly date)
    {
        return date >= StartDate && date <= EndDate;
    }
}