namespace FinanceManager.Application.UseCases.Settings;

using FinanceManager.Application.Interfaces;
using FinanceManager.Application.Models;
using FinanceManager.Application.UseCases;
using FinanceManager.Domain.Enums;

public sealed record UpdateUserPasswordResult(IEnumerable<UseCaseError> Errors) : UseCaseResult(Errors);
public sealed class UpdateUserPassword(IIdentityService identityService, IAuditLogRepository auditLog, IDataStore dataStore)
{
    public async Task<UpdateUserPasswordResult> ExecuteAsync(PasswordModel model)
    {
        var currentUser = await identityService.FindByEmailAsync(model.CurrentEmail) ?? throw new InvalidOperationException("Current user not found.");

        var success = await identityService.ChangePasswordAsync(model.CurrentEmail, model.CurrentPassword, model.NewPassword);
        if (!success)
        {
            return new UpdateUserPasswordResult([new UseCaseInvalidOperationError("Failed to update user password.")]);
        }

        await auditLog.LogAsync(currentUser, AuditedEvent.PasswordChanged, "Password has been changed.", DateTime.Now);
        await dataStore.SaveAsync();

        return new UpdateUserPasswordResult([]);
    }
}