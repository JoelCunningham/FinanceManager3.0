using FinanceManager.Domain.Enums;
using FinanceManager.Domain.Utilities;

namespace FinanceManager.Application.DTOs;

public sealed class BudgetCell(BudgetScope scope, int position, CategorySummary category, DateOnly startDate, decimal realAmount)
{
    public BudgetScope Scope { get; set; } = scope;
    public int Position { get; set; } = position;
    public CategorySummary Category { get; set; } = category;
    public DateOnly StartDate { get; set; } = startDate;

    public decimal RealAmount { get; set; } = realAmount;
    public IEnumerable<BudgetCellEntry> Entries { get; set; } = [];

    public decimal BudgetAmount => Entries.Sum(e => e.Amount);

    public string Label => ScopeHelper.GetScopePositionName(Scope, Position, StartDate);

    public bool IsOverBudget()
    {
        if (Category.IsIncome)
        {
            return RealAmount < BudgetAmount;
        }
        else
        {
            return -RealAmount > BudgetAmount;
        }
    }
}
