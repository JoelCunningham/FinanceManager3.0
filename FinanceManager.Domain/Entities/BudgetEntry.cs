namespace FinanceManager.Domain.Entities;

using FinanceManager.Domain.Constants;
using FinanceManager.Domain.Entities.Base;
using FinanceManager.Domain.Enums;

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

    public DateOnly StartDate => Period.Scope switch
    {
        BudgetScope.Weekly => Period.StartDate.AddDays(DateConstants.DAYS_IN_WEEK * PeriodPosition),
        BudgetScope.Fortnightly => Period.StartDate.AddDays(DateConstants.DAYS_IN_FORTNIGHT * PeriodPosition),
        BudgetScope.Monthly => Period.StartDate.AddMonths(PeriodPosition),
        _ => throw new NotSupportedException()
    };

    public DateOnly EndDate => Period.Scope switch
    {
        BudgetScope.Weekly => StartDate.AddDays(DateConstants.DAYS_IN_WEEK - 1),
        BudgetScope.Fortnightly => StartDate.AddDays(DateConstants.DAYS_IN_FORTNIGHT - 1),
        BudgetScope.Monthly => StartDate.AddMonths(1).AddDays(-1),
        _ => throw new NotSupportedException()
    };

    public decimal DailyAmount => Amount / (EndDate.DayNumber - StartDate.DayNumber + 1);
}