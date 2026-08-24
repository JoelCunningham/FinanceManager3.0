namespace FinanceManager.Application.UseCases.Auth;

using FinanceManager.Application.DTOs;
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
        UserSummary? user;

        if (model.Reason == ConfirmEmailReason.Registration)
        {
            if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Token))
            {
                return new ConfirmEmailResult([new UseCaseInvalidOperationError("Email and token are required.")]);
            }
            user = await identityService.ConfirmEmailAsync(model.Email, model.Token);
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
            user = await identityService.ChangeEmailAsync(model.Email, model.Token);
        }

        if (user is null)
        {
            return new ConfirmEmailResult([new UseCaseInvalidOperationError("Email confirmation failed. Invalid token or email.")]);
        }

        await auditLog.LogAsync(user.Id, AuditedEvent.EmailConfirmed, $"Email {model.Email} has been confirmed.", DateTime.Now);
        await dataStore.SaveAsync();

        return new ConfirmEmailResult([]);
    }
}