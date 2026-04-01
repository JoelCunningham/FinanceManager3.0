namespace FinanceManager.Domain.Entities;

using FinanceManager.Domain.Entities.Base;
using FinanceManager.Domain.Enums;

public class BudgetPeriod : IEntity
{
    public Guid Id { get; set; }

    public int Year { get; set; }
    public BudgetScope Scope { get; set; }

    public DateOnly StartDate => new(Year, 1, 1);
    public DateOnly EndDate => new(Year, 12, 31);
}