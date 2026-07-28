namespace FinanceManager.Domain.Enums;

public enum AuditedEvent
{
    AccountCreated,
    EmailConfirmed,
    PasswordResetRequested,
    PasswordResetCompleted,
    PasswordChanged,
    EmailChanged,
    AccountDeleted,
}
