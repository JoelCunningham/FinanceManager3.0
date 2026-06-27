namespace FinanceManager.Application.DTOs;

using FinanceManager.Domain.Entities;

public class CategorySummary
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Colour { get; set; }
    public Guid GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public string GroupColour { get; set; } = string.Empty;
    public string GroupIcon { get; set; } = string.Empty;
    public bool IsIncome { get; set; }

    public static CategorySummary FromCategory(Category category)
    {
        return new CategorySummary
        {
            Id = category.Id,
            Name = category.Name,
            Colour = category.Colour,
            GroupId = category.GroupId,
            GroupName = category.Group.Name,
            GroupColour = category.Group.Colour,
            GroupIcon = category.Group.Icon,
            IsIncome = category.Group.IsIncome
        };
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
                Icon = GroupIcon,
                Colour = GroupColour,
                IsIncome = IsIncome
            }
        };
    }
}
