namespace FinanceManager.Application.DTOs;

using FinanceManager.Domain.Entities;

public sealed class BudgetCellEntry()
{
    public Guid? EntityId { get; set; }
    public int Index { get; set; }
    public CategorySummary? Category { get; set; }
    public decimal Amount { get; set; }
    public string? Notes { get; set; }
    public int PeriodPosition { get; set; }
    public int Length { get; set; }

    public bool IsFirst => Index == 0;
    public bool IsLast => Index == Length - 1;

    public bool IsStartOfRow => Index % 4 == 0;
    public bool IsEndOfRow => (Index + 1) % 4 == 0;

    public static BudgetCellEntry FromBudgetEntry(BudgetEntry entry, int index)
    {
        return new BudgetCellEntry
        {
            EntityId = entry.Id,
            Category = CategorySummary.FromCategory(entry.Category),
            Amount = entry.Amount,
            Notes = entry.Notes,
            PeriodPosition = entry.PeriodPosition,
            Length = entry.Length,
           Index = index,
        };
    }

    public BudgetEntry ToBudgetEntry(BudgetPeriod period)
    {
        if (Category == null) throw new InvalidOperationException("Category must be provided.");

        return new BudgetEntry
        {
            Id = EntityId ?? Guid.NewGuid(),
            CategoryId = Category.Id,
            Category = Category.ToCategory(),
            Amount = Amount,
            Notes = Notes,
            PeriodId = period.Id,
            Period = period,
            PeriodPosition = PeriodPosition,
            Length = Length
        };
    }
}