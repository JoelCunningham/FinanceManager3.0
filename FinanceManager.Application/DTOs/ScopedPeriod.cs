namespace FinanceManager.Application.DTOs;

using FinanceManager.Domain.Enums;
using FinanceManager.Domain.Utilities;

public record ScopedPeriod : InnerPeriod
{
    public BudgetScope Scope { get; init; }
    public int Length { get; init; }
    public IEnumerable<InnerPeriod> InnerPeriods { get; init; }
    public string ScopeDescription { get; init; }

    public ScopedPeriod(BudgetScope scope, DateOnly containingDate, int length = 1)
    {
        if (length < 1) throw new ArgumentOutOfRangeException(nameof(length), "Length must be at least 1.");
        
        Scope = scope;
        Length = length;

        StartDate = ScopeHelper.GetPeriodStart(scope, containingDate);
        EndDate = ScopeHelper.GetPeriodEnd(scope, StartDate, length);

        InnerPeriods = GetInnerPeriods();
        ScopeDescription = ScopeHelper.GetScopeDescription(scope);
    }

    private List<InnerPeriod> GetInnerPeriods()
    {
        var innerPeriods = new List<InnerPeriod>();
        var currentStart = StartDate;
        for (int i = 0; i < Length; i++)
        {
            var currentEnd = ScopeHelper.GetPeriodEnd(Scope, currentStart);
            innerPeriods.Add(new InnerPeriod { StartDate = currentStart, EndDate = currentEnd });
            currentStart = currentEnd.AddDays(1);
        }
        return innerPeriods;
    }
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