namespace FinanceManager.Application.UseCases.Settings;

using FinanceManager.Application.Interfaces;
using FinanceManager.Application.Models;
using FinanceManager.Application.UseCases;
using FinanceManager.Domain.Enums;

public sealed record DeleteUserResult(IEnumerable<UseCaseError> Errors) : UseCaseResult(Errors);
public sealed class DeleteUser(IIdentityService identityService, IAuditLogRepository auditLog, IDataStore dataStore)
{
    public async Task<DeleteUserResult> ExecuteAsync()
    {
        var user = await identityService.DeleteUser();
        if (user is null)
        {
            return new DeleteUserResult([new UseCaseInvalidOperationError("Failed to delete user.")]);
        }

        await auditLog.LogAsync(user.Id, AuditedEvent.AccountDeleted, "User has been deleted.", DateTime.Now);
        await dataStore.SaveAsync();

        return new DeleteUserResult([]);
    }
}