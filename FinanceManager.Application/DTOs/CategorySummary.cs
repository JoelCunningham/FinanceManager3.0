namespace FinanceManager.Application.DTOs;

using FinanceManager.Domain.Entities;

public sealed record CategorySummary(
    Guid Id,
    string Name,
    Guid GroupId,
    string GroupName,
    bool IsIncome
)
{
    public static CategorySummary FromCategory(Category category)
    {
        return new CategorySummary(
            category.Id,
            category.Name,
            category.GroupId,
            category.Group.Name,
            category.Group.IsIncome
        );
    }

    public Category ToCategory()
    {
        return new Category
        {
            Id = Id,
            Name = Name,
            GroupId = GroupId,
            Group = new CategoryGroup
            {
                Id = GroupId,
                Name = GroupName,
                IsIncome = IsIncome
            }
        };
    }
}
