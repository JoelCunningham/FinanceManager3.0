namespace FinanceManager.Application.DTOs;

using FinanceManager.Domain.Entities;
using FinanceManager.Domain.Enums;

public class BudgetYearSummary
{
    public Guid EntityId { get; set; }
    public int Year { get; set; }
    public BudgetScope Scope { get; set; }

    public static BudgetYearSummary FromBudgetYear(BudgetYear budgetYear)
    {
        return new BudgetYearSummary
        {
            EntityId = budgetYear.Id,
            Year = budgetYear.Year,
            Scope = budgetYear.Scope
        };
    }
}
