namespace FinanceManager.Application.UseCases.Auth;

using FinanceManager.Application.Interfaces;
using FinanceManager.Application.Models;
using FinanceManager.Application.UseCases;
using FinanceManager.Domain.Enums;

public sealed record ResetPasswordResult(IEnumerable<UseCaseError> Errors) : UseCaseResult(Errors);
public sealed class ResetPassword(IIdentityService identityService, IAuditLogRepository auditLog)
{
    public async Task<ResetPasswordResult> ExecuteAsync(ResetPasswordModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Password) || string.IsNullOrWhiteSpace(model.ConfirmPassword))
        {
            return new ResetPasswordResult([new UseCaseInvalidOperationError("Password is required.")]);
        }

        if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Token))
        {
            return new ResetPasswordResult([new UseCaseInvalidOperationError("Email and token are required.")]);
        }

        var (user, errors) = await identityService.ResetPasswordAsync(model.Email, model.Token, model.Password);

        if (user is null)
        {
            var error = errors.FirstOrDefault();

            if (error is null)
            {
                return new ResetPasswordResult([new UseCaseUnexpectedError()]);
            }
            else
            {
                return new ResetPasswordResult([new UseCaseInvalidOperationError(error)]);
            }
        }

        await auditLog.LogAsync(user.Id, AuditedEvent.PasswordResetCompleted, "Password reset completed.", DateTime.Now);

        return new ResetPasswordResult([]);
    }
}