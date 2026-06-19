namespace FinanceManager.Application.DTOs;

using FinanceManager.Domain.Entities;

public class CategoryPeriodDetails : CategorySummary
{
    public decimal PeriodSpend { get; set; }
    public decimal PeriodBudget { get; set; }
    public decimal SignedPeriodTotal => IsIncome ? PeriodSpend : -PeriodSpend;

    public static CategoryPeriodDetails FromCategory(Category category, decimal spend, decimal budget)
    {
        return new CategoryPeriodDetails
        {
            Id = category.Id,
            Name = category.Name,
            Colour = category.Colour,
            GroupId = category.GroupId,
            GroupName = category.Group.Name,
            GroupColour = category.Group.Colour,
            IsIncome = category.Group.IsIncome,
            PeriodSpend = spend,
            PeriodBudget = budget,
        };
    }
}
