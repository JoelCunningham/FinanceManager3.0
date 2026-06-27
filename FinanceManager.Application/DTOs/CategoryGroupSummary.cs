namespace FinanceManager.Application.DTOs;

using FinanceManager.Domain.Entities;

public class CategoryGroupSummary
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Colour { get; set; }
    public required string Icon { get; set; }
    public bool IsIncome { get; set; }
    public int CategoryCount { get; set; }

    public static CategoryGroupSummary Empty(Guid? id = null)
    {
        return new CategoryGroupSummary
        {
            Id = id ?? Guid.Empty,
            Name = string.Empty,
            Colour = string.Empty,
            Icon = string.Empty,
            IsIncome = false,
            CategoryCount = 0
        };
    }

    public static CategoryGroupSummary FromCategoryGroup(CategoryGroup group)
    {
        return new CategoryGroupSummary
        {
            Id = group.Id,
            Name = group.Name,
            Colour = group.Colour,
            Icon = group.Icon,
            IsIncome = group.IsIncome,
            CategoryCount = group.Categories?.Count ?? 0
        };
    }
}