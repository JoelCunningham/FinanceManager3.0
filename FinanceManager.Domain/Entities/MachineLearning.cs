namespace FinanceManager.Domain.Entities;

using FinanceManager.Domain.Entities.Base;

public sealed class MachineLearning : UserOwnedEntity
{
    public Guid CategoryId { get; set; }
    public required Category Category { get; set; }

    public required string NormalisedDescription { get; set; }
    public DateTime LastUsed { get; set; }
}
