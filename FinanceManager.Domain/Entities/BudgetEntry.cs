namespace FinanceManager.Domain.Entities;

using FinanceManager.Domain.Entities.Base;
using FinanceManager.Domain.Utilities;

public class BudgetEntry : IEntity
{
    public Guid Id { get; set; }

    public Guid PeriodId { get; set; }
    public required BudgetPeriod Period { get; set; }

    public Guid CategoryId { get; set; }
    public required Category Category { get; set; }

    public int Length { get; set; }
    public int PeriodPosition { get; set; }

    public decimal Amount { get; set; }
    public string? Notes { get; set; }

    public DateOnly StartDate => ScopeHelper.GetPeriodStart(Period.Scope, Period.StartDate, PeriodPosition);
    public DateOnly EndDate => ScopeHelper.GetPeriodEnd(Period.Scope, StartDate);
}