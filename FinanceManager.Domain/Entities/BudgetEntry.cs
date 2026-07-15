namespace FinanceManager.Domain.Entities;

using FinanceManager.Domain.Entities.Base;
using FinanceManager.Domain.Utilities;

public class BudgetEntry : UserOwnedEntity
{
    public Guid BudgetYearId { get; set; }
    public required BudgetYear BudgetYear { get; set; }

    public Guid CategoryId { get; set; }
    public required Category Category { get; set; }

    public int Length { get; set; }
    public int ScopePosition { get; set; }

    public decimal Amount { get; set; }
    public string? Notes { get; set; }

    public DateOnly StartDate => ScopeHelper.GetPeriodStart(BudgetYear.Scope, BudgetYear.StartDate, ScopePosition);
    public DateOnly EndDate => ScopeHelper.GetRangeEnd(BudgetYear.Scope, StartDate, Length);
}