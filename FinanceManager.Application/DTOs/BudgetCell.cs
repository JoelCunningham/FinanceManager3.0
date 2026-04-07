namespace FinanceManager.Application.DTOs;

using FinanceManager.Domain.Enums;

public sealed class BudgetCell(int index, DateOnly startDate, BudgetScope scope)
{
    public int Index { get; set; } = index;
    public DateOnly StartDate { get; set; } = startDate;
    public BudgetScope? Scope { get; set; } = scope;
    public List<BudgetCellEntry> Entries { get; set; } = [];

    public string Label => Scope switch
    {
        BudgetScope.Monthly => StartDate.ToString("MMM"),
        BudgetScope.Fortnightly => $"F{Index + 1:00} ({StartDate:dd/MM})",
        BudgetScope.Weekly => $"W{Index + 1:00} ({StartDate:dd/MM})",
        _ => StartDate.ToString("dd/MM/yyyy")
    };
}