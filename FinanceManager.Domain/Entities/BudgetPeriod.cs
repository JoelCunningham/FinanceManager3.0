namespace FinanceManager.Domain.Entities;

using FinanceManager.Domain.Constants;
using FinanceManager.Domain.Entities.Base;
using FinanceManager.Domain.Enums;

public class BudgetPeriod : IEntity
{
    public Guid Id { get; set; }

    public int Length { get; set; }
    public BudgetScope Scope { get; set; }

    public DateOnly StartDate { get; set; }

    public ICollection<BudgetEntry> Entries { get; set; } = [];

    public DateOnly EndDate => Scope switch
    {
        BudgetScope.Weekly => StartDate.AddDays(DateConstants.DAYS_IN_WEEK * Length - 1),
        BudgetScope.Fortnightly => StartDate.AddDays(DateConstants.DAYS_IN_FORTNIGHT * Length - 1),
        BudgetScope.Monthly => StartDate.AddMonths(Length).AddDays(-1),
        _ => throw new NotSupportedException()
    };
}