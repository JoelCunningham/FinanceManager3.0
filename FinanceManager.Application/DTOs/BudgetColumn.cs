namespace FinanceManager.Application.DTOs;

using FinanceManager.Domain.Enums;
using FinanceManager.Domain.Utilities;

public sealed class BudgetColumn(DateOnly startDate, BudgetScope scope, List<BudgetCell> cells)
{
    public BudgetScope? Scope { get; set; } = scope;
    public DateOnly StartDate { get; set; } = startDate;

    public List<BudgetCell> Cells { get; set; } = cells;

    public DateOnly EndDate => ScopeHelper.GetPeriodEnd(Scope ?? BudgetScope.Monthly, StartDate);

    public string Label => ScopeHelper.GetPeriodName(StartDate, Scope ?? BudgetScope.Monthly, false);
    public string? SubLabel => Scope switch
    {
        BudgetScope.Monthly => null,
        BudgetScope.Fortnightly => $"{StartDate:dd/MM}",
        BudgetScope.Weekly => $"{StartDate:dd/MM}",
        _ => null
    };

    public bool IsFuturePeriod()
    {
        return DateOnly.FromDateTime(DateTime.Today) < StartDate;
    }
}

public sealed record CategoryGroupRow(string Name, string Colour, string Icon, IReadOnlyList<CategorySummary> Categories);