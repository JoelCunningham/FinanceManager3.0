namespace FinanceManager.Application.DTOs;

using FinanceManager.Domain.Enums;
using FinanceManager.Domain.Utilities;

public sealed class BudgetCell(int index, DateOnly startDate, BudgetScope scope)
{
    public int Index { get; set; } = index;
    public DateOnly StartDate { get; set; } = startDate;
    public BudgetScope? Scope { get; set; } = scope;
    public List<BudgetCellEntry> Entries { get; set; } = [];

    public DateOnly EndDate => ScopeHelper.GetPeriodEnd(Scope ?? BudgetScope.Monthly, StartDate);

    private DateOnly Today = DateOnly.FromDateTime(DateTime.Today);

    public string Label => Scope switch
    {
        BudgetScope.Monthly => StartDate.ToString("MMM"),
        BudgetScope.Fortnightly => $"F{Index + 1:00}",
        BudgetScope.Weekly => $"W{Index + 1:00}",
        _ => StartDate.ToString("dd/MM/yyyy")
    };

    public string? SubLabel => Scope switch
    {
        BudgetScope.Monthly => null,
        BudgetScope.Fortnightly => $"{StartDate:dd/MM}",
        BudgetScope.Weekly => $"{StartDate:dd/MM}",
        _ => null
    };

    public bool IsCurrentPeriod()
    {
        return Today >= StartDate && Today <= EndDate;
    }

    public bool IsFuturePeriod()
    {
        return Today < StartDate;
    }
}