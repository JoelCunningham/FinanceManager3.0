namespace FinanceManager.Application.DTOs;

public class BudgetCategoryUsage
{
    public required CategorySummary Summary { get; set; }
    public decimal Proportion { get; set; }
    public decimal Amount { get; set; }
    public decimal BudgetedAmount { get; set; }

    public string DisplayName => $"{Summary.Name} · {Summary.GroupName}";
}
