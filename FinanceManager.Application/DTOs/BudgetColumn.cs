namespace FinanceManager.Application.DTOs;

using FinanceManager.Domain.Enums;
using FinanceManager.Domain.Utilities;

public sealed class BudgetColumn(int position, DateOnly startDate, BudgetScope scope, List<BudgetCell> cells)
{
    public BudgetScope? Scope { get; set; } = scope;
    public int Position { get; set; } = position;
    public DateOnly StartDate { get; set; } = startDate;

    public List<BudgetCell> Cells { get; set; } = cells;

    public DateOnly EndDate => ScopeHelper.GetPeriodEnd(Scope ?? BudgetScope.Monthly, StartDate);

    public string Label => ScopeHelper.GetScopePositionName(Scope ?? BudgetScope.Monthly, Position, StartDate);
    public string? SubLabel => Scope switch
    {
        BudgetScope.Monthly => null,
        BudgetScope.Fortnightly => $"{StartDate:dd/MM}",
        BudgetScope.Weekly => $"{StartDate:dd/MM}",
        _ => null
    };

    public bool IsCurrentPeriod()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        return today >= StartDate && today <= EndDate;
    }

    public bool IsFuturePeriod()
    {
        return DateOnly.FromDateTime(DateTime.Today) < StartDate;
    }
}