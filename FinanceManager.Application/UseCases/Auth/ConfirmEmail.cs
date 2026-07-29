namespace FinanceManager.Application.UseCases.Auth;

using FinanceManager.Application.Enums;
using FinanceManager.Application.Interfaces;
using FinanceManager.Application.Models;
using FinanceManager.Application.UseCases;
using FinanceManager.Domain.Enums;

public sealed record ConfirmEmailResult(IEnumerable<UseCaseError> Errors) : UseCaseResult(Errors);
public sealed class ConfirmEmail(IIdentityService identityService, IAuditLogRepository auditLog, IDataStore dataStore)
{
    public async Task<ConfirmEmailResult> ExecuteAsync(ConfirmEmailModel model)
    {
        Guid? userId;

        if (model.Reason == ConfirmEmailReason.Registration)
        {
            if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Token))
            {
                return new ConfirmEmailResult([new UseCaseInvalidOperationError("Email and token are required.")]);
            }
            userId = await identityService.ConfirmEmailAsync(model.Email, model.Token);
        }
        else
        {
            if (string.IsNullOrWhiteSpace(model.OldEmail) || string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Token))
            {
                return new ConfirmEmailResult([new UseCaseInvalidOperationError("Old email, new email, and token are required.")]);
            }
            if (await identityService.FindByEmailAsync(model.Email) is not null)
            {
                return new ConfirmEmailResult([]);
            }
            userId = await identityService.ChangeEmailAsync(model.OldEmail, model.Email, model.Token);
        }

        if (!userId.HasValue)
        {
            return new ConfirmEmailResult([new UseCaseInvalidOperationError("Email confirmation failed. Invalid token or email.")]);
        }

        await auditLog.LogAsync(userId.Value, AuditedEvent.EmailConfirmed, $"Email {model.Email} has been confirmed.", DateTime.Now);
        await dataStore.SaveAsync();

        return new ConfirmEmailResult([]);
    }
}