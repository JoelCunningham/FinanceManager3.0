namespace FinanceManager.Application.UseCases.Auth;

using FinanceManager.Application.Interfaces;
using FinanceManager.Application.Models;
using FinanceManager.Application.UseCases;

public sealed record ValidateResetTokenResult(IEnumerable<UseCaseError> Errors) : UseCaseResult(Errors);
public sealed class ValidateResetToken(IIndentityService identityService)
{
    public async Task<ValidateResetTokenResult> ExecuteAsync(ResetPasswordModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Token))
        {
            return new ValidateResetTokenResult([new UseCaseInvalidOperationError("Invalid request.")]);
        }

        if (!await identityService.VerifyUserResetTokenAsync(model.Email, model.Token))
        {
            return new ValidateResetTokenResult([new UseCaseInvalidOperationError("Invalid or expired token.")]);
        }

        return new ValidateResetTokenResult([]);
    }
}