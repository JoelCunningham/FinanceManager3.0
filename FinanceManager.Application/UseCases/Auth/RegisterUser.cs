namespace FinanceManager.Application.UseCases.Auth;

using FinanceManager.Application.Constants.Navigation;
using FinanceManager.Application.Interfaces;
using FinanceManager.Application.Models;
using FinanceManager.Application.UseCases;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

public sealed record RegisterUserResult(IEnumerable<UseCaseError> Errors) : UseCaseResult(Errors);
public sealed class RegisterUser(IIndentityService identityService, IUserEmailService emailService)
{
    public async Task<RegisterUserResult> ExecuteAsync(RegisterUserModel model)
    {
        var (success, errors) = await identityService.CreateUserAsync(model.Name, model.Email, model.Password);

        if (!success)
        {
            var error = errors.FirstOrDefault();

            if (error is null)
            {
                return new RegisterUserResult([new UseCaseUnexpectedError()]);
            }

            if (error.Contains("Username") && error.Contains("already taken"))
            {
                error = error.Replace("Username", "Email").Replace("already taken", "already registered");
            }

            return new RegisterUserResult([new UseCaseInvalidOperationError(error)]);
        }

        var token = await identityService.GenerateEmailConfirmationTokenAsync(model.Email);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

        var confirmationLink = QueryHelpers.AddQueryString($"{model.Origin}{Pages.ConfirmEmail}", new Dictionary<string, string?>
        {
            [Parameters.Email] = model.Email,
            [Parameters.Token] = encodedToken
        });

        await emailService.SendEmailConfirmationAsync(model.Name, model.Email, confirmationLink);

        return new RegisterUserResult([]);
    }
}
