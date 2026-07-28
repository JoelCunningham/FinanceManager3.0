namespace FinanceManager.Application.UseCases.Auth;

using FinanceManager.Application.Interfaces;
using FinanceManager.Application.Models;
using FinanceManager.Application.UseCases;
using FinanceManager.Domain.Enums;

public sealed record ConfirmEmailResult(IEnumerable<UseCaseError> Errors) : UseCaseResult(Errors);
public sealed class ConfirmEmail(IIndentityService identityService, IAuditLogRepository auditLog, IDataStore dataStore)
{
    public async Task<ConfirmEmailResult> ExecuteAsync(ConfirmEmailModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Token))
        {
            return new ConfirmEmailResult([new UseCaseInvalidOperationError("Email and token are required.")]);
        }

        var userId = await identityService.ConfirmEmailAsync(model.Email, model.Token);

        if (!userId.HasValue)
        {
            return new ConfirmEmailResult([new UseCaseInvalidOperationError("Email confirmation failed. Invalid token or email.")]);
        }

        await auditLog.LogAsync(userId.Value, AuditedEvent.EmailConfirmed, $"Email {model.Email} has been confirmed.", DateTime.Now);
        await dataStore.SaveAsync();

        return new ConfirmEmailResult([]);
    }
}