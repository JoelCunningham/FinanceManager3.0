namespace FinanceManager.Domain.Entities.Base;

public abstract class UserOwnedEntity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
}
