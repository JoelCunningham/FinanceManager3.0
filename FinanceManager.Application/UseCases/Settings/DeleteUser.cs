namespace FinanceManager.Application.UseCases.Settings;

using FinanceManager.Application.Interfaces;
using FinanceManager.Application.Models;
using FinanceManager.Application.UseCases;
using FinanceManager.Domain.Enums;

public sealed record DeleteUserResult(IEnumerable<UseCaseError> Errors) : UseCaseResult(Errors);
public sealed class DeleteUser(IIdentityService identityService, IAuditLogRepository auditLog, IDataStore dataStore)
{
    public async Task<DeleteUserResult> ExecuteAsync(DeleteModel model)
    {
        var currentUser = await identityService.FindByEmailAsync(model.CurrentEmail) ?? throw new InvalidOperationException("Current user not found.");

        var success = await identityService.DeleteUser(model.CurrentEmail);
        if (!success)
        {
            return new DeleteUserResult([new UseCaseInvalidOperationError("Failed to delete user.")]);
        }

        await auditLog.LogAsync(currentUser, AuditedEvent.AccountDeleted, "User has been deleted.", DateTime.Now);
        await dataStore.SaveAsync();

        return new DeleteUserResult([]);
    }
}