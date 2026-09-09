namespace FinanceManager.Application.DTOs;

using FinanceManager.Domain.Entities;

public sealed class BudgetCellEntry()
{
    public Guid? EntityId { get; set; }

    public string? Name { get; set; }
    public required CategorySummary Category { get; set; }
    public decimal Amount { get; set; }

    public int ScopePosition { get; set; }
    public int Length { get; set; }

    public static BudgetCellEntry FromBudgetEntry(BudgetEntry entry)
    {
        return new BudgetCellEntry
        {
            EntityId = entry.Id,
            Name = entry.Notes,
            Category = CategorySummary.FromCategory(entry.Category),
            Amount = entry.Amount,
            ScopePosition = entry.ScopePosition,
            Length = entry.Length
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
            Notes = Name,
            BudgetYearId = budgetYear.Id,
            BudgetYear = null!,
            ScopePosition = ScopePosition,
            Length = Length
        };
    }
}
