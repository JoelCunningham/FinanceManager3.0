namespace FinanceManager.Application.DTOs;

using FinanceManager.Domain.Entities;
using FinanceManager.Domain.Enums;

public sealed class BudgetCell(int index, string label)
{
    public int Index { get; } = index;
    public string Label { get; } = label;
    public CellScopeType ScopeType { get; set; } = CellScopeType.None;
    public BudgetScope? Scope { get; set; } = null;
    public List<BudgetEntry> Entries { get; set; } = [];
}

public enum CellScopeType
{
    Current,
    Other,
    Mixed,
    None
}