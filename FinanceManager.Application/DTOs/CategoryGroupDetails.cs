using FinanceManager.Domain.Entities;
using FinanceManager.Domain.Enums;

namespace FinanceManager.Application.DTOs;

public class CategoryGroupDetails : CategoryGroupSummary
{
    public BudgetScope? PeriodScope { get; set; }
    public required string PeriodName { get; set; }
    public IEnumerable<CategoryPeriodDetails> Categories { get; set; } = [];

    public static CategoryGroupDetails FromCategoryGroup(CategoryGroup group, BudgetScope? periodScope, string periodName, IEnumerable<CategoryPeriodDetails> categories)
    {
        return new CategoryGroupDetails
        {
            Id = group.Id,
            Name = group.Name,
            Colour = group.Colour,
            Icon = group.Icon,
            IsIncome = group.IsIncome,
            CategoryCount = group.Categories?.Count ?? 0,
            PeriodScope = periodScope,
            PeriodName = periodName,
            Categories = categories
        };
    }
}