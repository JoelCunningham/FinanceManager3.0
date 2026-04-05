namespace FinanceManager.Application.DTOs;

using FinanceManager.Domain.Entities;

public sealed record CategorySummary(
    Guid Id,
    string Name,
    string Colour,
    Guid GroupId,
    string GroupName,
    string GroupColour,
    bool IsIncome
)
{
    public static CategorySummary FromCategory(Category category)
    {
        return new CategorySummary(
            category.Id,
            category.Name,
            category.Colour,
            category.GroupId,
            category.Group.Name,
            category.Group.Colour,
            category.Group.IsIncome
        );
    }

    public Category ToCategory()
    {
        return new Category
        {
            Id = Id,
            Name = Name,
            Colour = Colour,
            GroupId = GroupId,
            Group = new CategoryGroup
            {
                Id = GroupId,
                Name = GroupName,
                IsIncome = IsIncome,
                Colour = GroupColour
            }
        };
    }
}
