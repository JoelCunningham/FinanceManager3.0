namespace FinanceManager.Application.DTOs;

using FinanceManager.Domain.Enums;
using FinanceManager.Domain.Utilities;

public record ScopedPeriod
{
    public BudgetScope Scope { get; init; }
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
    public string ScopeDescription => ScopeHelper.GetScopeDescription(Scope);
    public string PeriodDescription => ScopeHelper.GetPeriodName(EndDate, Scope);

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