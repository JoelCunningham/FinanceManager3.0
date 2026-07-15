namespace FinanceManager.Domain.Entities;

using FinanceManager.Domain.Entities.Base;

public sealed class Category : UserOwnedEntity
{
    public Guid GroupId { get; set; }
    public required CategoryGroup Group { get; set; }

    public required string Name { get; set; }
    public required string Colour { get; set; }
}