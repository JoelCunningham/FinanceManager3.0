namespace FinanceManager.Domain.Entities;

using FinanceManager.Domain.Entities.Base;
using FinanceManager.Domain.Enums;

public sealed class AuditLog : UserOwnedEntity
{
    public AuditedEvent Event { get; set; }
    public string? Description { get; set; }
    public DateTime Date { get; set; }

    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}