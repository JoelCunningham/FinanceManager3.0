using FinanceManager.Domain.Enums;

namespace FinanceManager.Application.Interfaces;

public interface IAuditLogRepository
{
    Task LogAsync(Guid userId, AuditedEvent action, string? description, DateTime date);
}
