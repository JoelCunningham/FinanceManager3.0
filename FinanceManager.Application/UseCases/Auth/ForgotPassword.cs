namespace FinanceManager.Application.UseCases.Auth;

using FinanceManager.Application.Constants.Navigation;
using FinanceManager.Application.Interfaces;
using FinanceManager.Application.Models;
using FinanceManager.Application.UseCases;
using FinanceManager.Domain.Enums;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

public sealed record ForgotPasswordResult(IEnumerable<UseCaseError> Errors) : UseCaseResult(Errors);
public sealed class ForgotPassword(IIdentityService identityService, IUserEmailService emailService, IAuditLogRepository auditLog)
{
    public async Task<ForgotPasswordResult> ExecuteAsync(ForgotPasswordModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Email))
        {
            return new ForgotPasswordResult([new UseCaseInvalidOperationError("Email is required.")]);
        }

        var user = await identityService.FindByEmailAsync(model.Email);

        if (user is null)
        {
            return new ForgotPasswordResult([new UseCaseInvalidOperationError("User not found.")]);
        }

        var token = await identityService.GeneratePasswordResetTokenAsync(user.Email);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

        var resetLink = QueryHelpers.AddQueryString($"{model.Origin}{Pages.ResetPassword}", new Dictionary<string, string?>
        {
            [Parameters.Email] = user.Email,
            [Parameters.Token] = encodedToken
        });

        await emailService.SendPasswordResetAsync(user.Email, user.Email, resetLink);
        await auditLog.LogAsync(user.Id, AuditedEvent.PasswordResetRequested, "Password reset requested.", DateTime.Now);

        return new ForgotPasswordResult([]);
    }
}