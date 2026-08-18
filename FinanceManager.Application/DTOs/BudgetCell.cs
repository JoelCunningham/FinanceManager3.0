namespace FinanceManager.Application.DTOs;

using FinanceManager.Domain.Enums;
using FinanceManager.Domain.Utilities;

public sealed class BudgetCell(BudgetScope scope, CategorySummary category, DateOnly startDate)
{
    public BudgetScope Scope { get; set; } = scope;
    public CategorySummary Category { get; set; } = category;
    public DateOnly StartDate { get; set; } = startDate;

    public IEnumerable<TransactionSummary> Transactions { get; set; } = [];
    public IEnumerable<BudgetCellEntry> BudgetEntries { get; set; } = [];

    public decimal RealAmount => Transactions.Sum(t => t.Amount);
    public decimal BudgetAmount => BudgetEntries.Sum(e => e.Amount);

    public string Label => ScopeHelper.GetPeriodName(StartDate, Scope);
}
