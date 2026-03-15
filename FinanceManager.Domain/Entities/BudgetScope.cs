namespace FinanceManager.Domain.Entities;

using FinanceManager.Domain.Entities.Base;
using FinanceManager.Domain.Enums;

public class BudgetScope : IEntity
{
    public Guid Id { get; set; }

    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }

    public Scope Scope { get; set; }

    public ICollection<BudgetEntry> Entries { get; set; } = [];
}