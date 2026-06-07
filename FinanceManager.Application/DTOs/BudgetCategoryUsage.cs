namespace FinanceManager.Application.DTOs;

public class BudgetCategoryUsage
{
    public string CategoryName { get; set; } = default!;
    public string GroupName { get; set; } = default!;
    public decimal Proportion { get; set; }
    public decimal Amount { get; set; }
    public decimal BudgetedAmount { get; set; }

    public string DisplayName => $"{GroupName} - {CategoryName}";
}
