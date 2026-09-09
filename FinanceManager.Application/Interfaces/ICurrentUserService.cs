namespace FinanceManager.Application.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
}