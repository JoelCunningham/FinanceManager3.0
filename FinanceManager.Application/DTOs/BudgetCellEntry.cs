namespace FinanceManager.Application.DTOs;

using FinanceManager.Domain.Entities;

public sealed class BudgetCellEntry()
{
    public Guid? EntityId { get; set; }
    public CategorySummary? Category { get; set; }
    public decimal Amount { get; set; }
    public string? Notes { get; set; }
    public int Index { get; set; }

    public int OverallPeriodPosition { get; set; }
    public int OverallLength { get; set; }

    public int PeriodPosition => OverallPeriodPosition + Index;

    public bool IsFirst => Index == 0;
    public bool IsStartOfRow => PeriodPosition % ROW_LENGTH == 0;
    public bool IsFirstInRow => IsFirst || IsStartOfRow;

    public int RowLength => Math.Min(ROW_LENGTH - (PeriodPosition % ROW_LENGTH), OverallLength - Index);

    private const int ROW_LENGTH = 4;

    public static BudgetCellEntry FromBudgetEntry(BudgetEntry entry, int index)
    {
        return new BudgetCellEntry
        {
            EntityId = entry.Id,
            Category = CategorySummary.FromCategory(entry.Category),
            Amount = entry.Amount,
            Notes = entry.Notes,
            OverallPeriodPosition = entry.PeriodPosition,
            OverallLength = entry.Length,
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
            PeriodPosition = OverallPeriodPosition,
            Length = OverallLength
        };
    }
}