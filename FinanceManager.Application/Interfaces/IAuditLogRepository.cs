namespace FinanceManager.Application.Interfaces;

using FinanceManager.Domain.Enums;

public interface IAuditLogRepository
{
    Task LogAsync(Guid userId, AuditedEvent action, string? description, DateTime date);
}
