namespace FinanceManager.Application.UseCases.Auth;

using FinanceManager.Application.Constants.Navigation;
using FinanceManager.Application.Interfaces;
using FinanceManager.Application.Models;
using FinanceManager.Application.UseCases;
using FinanceManager.Domain.Enums;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

public sealed record ForgotPasswordResult(IEnumerable<UseCaseError> Errors) : UseCaseResult(Errors);
public sealed class ForgotPassword(IIndentityService identityService, IUserEmailService emailService, IAuditLogRepository auditLog, IDataStore dataStore)
{
    public async Task<ForgotPasswordResult> ExecuteAsync(ForgotPasswordModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Email))
        {
            return new ForgotPasswordResult([new UseCaseInvalidOperationError("Email is required.")]);
        }

        var userId = await identityService.FindByEmailAsync(model.Email);

        if (!userId.HasValue)
        {
            return new ForgotPasswordResult([new UseCaseInvalidOperationError("User not found.")]);
        }

        var token = await identityService.GeneratePasswordResetTokenAsync(model.Email);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

        var resetLink = QueryHelpers.AddQueryString($"{model.Origin}{Pages.ResetPassword}", new Dictionary<string, string?>
        {
            [Parameters.Email] = model.Email,
            [Parameters.Token] = encodedToken
        });

        await emailService.SendPasswordResetAsync(model.Email, model.Email, resetLink);

        await auditLog.LogAsync(userId.Value, AuditedEvent.PasswordResetRequested, "Password reset requested.", DateTime.Now);
        await dataStore.SaveAsync();

        return new ForgotPasswordResult([]);
    }
}