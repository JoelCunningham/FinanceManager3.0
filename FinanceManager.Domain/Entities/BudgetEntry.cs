namespace FinanceManager.Domain.Entities;

using FinanceManager.Domain.Entities.Base;
using FinanceManager.Domain.Enums;

public class BudgetEntry : IEntity
{
    public Guid Id { get; set; }

    public Guid CategoryId { get; set; }
    public required Category Category { get; set; }

    public decimal Amount { get; set; }
    public BudgetPeriod Period { get; set; }
    public DateOnly StartDate { get; set; }

    public string? Notes { get; set; }
}
