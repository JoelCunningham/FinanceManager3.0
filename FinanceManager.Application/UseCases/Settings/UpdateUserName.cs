namespace FinanceManager.Application.UseCases.Settings;

using FinanceManager.Application.Interfaces;
using FinanceManager.Application.Models;
using FinanceManager.Application.UseCases;
using FinanceManager.Domain.Enums;

public sealed record UpdateUserNameResult(IEnumerable<UseCaseError> Errors) : UseCaseResult(Errors);
public sealed class UpdateUserName(IIdentityService identityService, IAuditLogRepository auditLog, IDataStore dataStore)
{
    public async Task<UpdateUserNameResult> ExecuteAsync(ProfileModel model)
    {
        if (model.CurrentName == model.NewName) return new UpdateUserNameResult([]);

        var user = await identityService.ChangeNameAsync(model.NewName);
        if (user is null)
        {
            return new UpdateUserNameResult([new UseCaseInvalidOperationError("Failed to update user name.")]);
        }

        await auditLog.LogAsync(user.Id, AuditedEvent.NameChanged, $"Name has been changed from {model.CurrentName} to {model.NewName}.", DateTime.Now);
        await dataStore.SaveAsync();

        return new UpdateUserNameResult([]);
    }
}