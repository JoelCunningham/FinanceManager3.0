namespace FinanceManager.Domain.Entities;

using FinanceManager.Domain.Entities.Base;

public sealed class CategoryGroup : UserOwnedEntity
{
    public bool IsIncome { get; set; }
    public required string Name { get; set; }
    public required string Colour { get; set; }
    public required string Icon { get; set; }

    public ICollection<Category>? Categories { get; set; }
}