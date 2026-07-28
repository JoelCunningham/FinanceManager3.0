namespace FinanceManager.Application.UseCases.Auth;

using FinanceManager.Application.Constants.Navigation;
using FinanceManager.Application.Interfaces;
using FinanceManager.Application.Models;
using FinanceManager.Application.UseCases;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

public sealed record ForgotPasswordResult(IEnumerable<UseCaseError> Errors) : UseCaseResult(Errors);
public sealed class ForgotPassword(IIndentityService identityService, IUserEmailService emailService)
{
    public async Task<ForgotPasswordResult> ExecuteAsync(ForgotPasswordModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Email))
        {
            return new ForgotPasswordResult([new UseCaseInvalidOperationError("Email is required.")]);
        }

        var exists = await identityService.FindByEmailAsync(model.Email);

        if (!exists)
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

        return new ForgotPasswordResult([]);
    }
}