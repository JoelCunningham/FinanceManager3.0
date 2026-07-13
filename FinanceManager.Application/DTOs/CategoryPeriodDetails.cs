namespace FinanceManager.Application.DTOs;

using FinanceManager.Domain.Entities;

public class CategoryPeriodDetails : CategorySummary
{
    public decimal PeriodSpend { get; set; }
    public decimal PeriodBudget { get; set; }

    public int TotalTransactions { get; set; }

    public decimal SignedPeriodTotal => IsIncome ? PeriodSpend : -PeriodSpend;
    public decimal? PeriodProportion => GetProportion(PeriodBudget, SignedPeriodTotal);

    public static CategoryPeriodDetails FromCategory(Category category, decimal spend, decimal budget, int totalTransactions)
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
            TotalTransactions = totalTransactions,
        };
    }

    private static decimal? GetProportion(decimal budget, decimal actual)
    {
        if (budget == 0 && actual == 0) return null;
        if (budget == 0 && actual != 0) return decimal.MaxValue;

        return actual / budget;
    }
}
