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
        var user = await identityService.ChangePasswordAsync(model.CurrentPassword, model.NewPassword);
        if (user is null)
        {
            return new UpdateUserPasswordResult([new UseCaseInvalidOperationError("Failed to update user password.")]);
        }

        await auditLog.LogAsync(user.Id, AuditedEvent.PasswordChanged, "Password has been changed.", DateTime.Now);
        await dataStore.SaveAsync();

        return new UpdateUserPasswordResult([]);
    }
}