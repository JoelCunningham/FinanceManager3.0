namespace FinanceManager.Application.DTOs;

using FinanceManager.Domain.Entities;

public sealed class BudgetCellEntry()
{
    public Guid? EntityId { get; set; }

    public int XIndex { get; set; }
    public int YIndex { get; set; }

    public CategorySummary? Category { get; set; }
    public decimal Amount { get; set; }
    public decimal RealAmount { get; set; }
    public string? Notes { get; set; }

    public int OverallScopePosition { get; set; }
    public int OverallLength { get; set; }

    public int ScopePosition => OverallScopePosition + XIndex;

    public bool IsFirst => XIndex == 0;

    public static BudgetCellEntry FromBudgetEntry(BudgetEntry entry, int xIndex, int yIndex, decimal realAmount)
    {
        return new BudgetCellEntry
        {
            EntityId = entry.Id,
            XIndex = xIndex,
            YIndex = yIndex,
            Category = CategorySummary.FromCategory(entry.Category),
            Amount = entry.Amount,
            RealAmount = realAmount,
            Notes = entry.Notes,
            OverallScopePosition = entry.ScopePosition,
            OverallLength = entry.Length
        };
    }

    public BudgetEntry ToBudgetEntry(BudgetYear budgetYear)
    {
        if (Category == null) throw new InvalidOperationException("Category must be provided.");

        return new BudgetEntry
        {
            Id = EntityId ?? Guid.NewGuid(),
            CategoryId = Category.Id,
            Category = null!,
            Amount = Amount,
            Notes = Notes,
            BudgetYearId = budgetYear.Id,
            BudgetYear = budgetYear,
            ScopePosition = OverallScopePosition,
            Length = OverallLength
        };
    }

    public bool IsOverBudget()
    {
        if (Category == null) throw new InvalidOperationException("Category must be provided.");

        if (Category.IsIncome)
        {
            return RealAmount < Amount;
        }
        else
        {
            return -RealAmount > Amount;
        }
    }
}
