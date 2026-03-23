namespace FinanceManager.Application.DTOs;

using FinanceManager.Domain.Entities;

public sealed record CategoryGroupSummary(
    Guid Id, 
    string Name, 
    bool IsIncome
)
{
    public static CategoryGroupSummary FromCategoryGroup(CategoryGroup group)
    {
        return new CategoryGroupSummary(
            group.Id,
            group.Name,
            group.IsIncome
        );
    }
}