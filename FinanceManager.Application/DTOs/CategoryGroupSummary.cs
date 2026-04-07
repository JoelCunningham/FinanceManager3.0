namespace FinanceManager.Application.DTOs;

using FinanceManager.Domain.Entities;

public sealed record CategoryGroupSummary(
    Guid Id, 
    string Name,
    string Colour,
    bool IsIncome,
    int CategoryCount
)
{
    public static CategoryGroupSummary FromCategoryGroup(CategoryGroup group)
    {
        return new CategoryGroupSummary(
            group.Id,
            group.Name,
            group.Colour,
            group.IsIncome,
            group.Categories?.Count ?? 0
        );
    }
}