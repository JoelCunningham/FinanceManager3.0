namespace FinanceManager.Application.UseCases.Auth;

using FinanceManager.Application.Constants.Navigation;
using FinanceManager.Application.Interfaces;
using FinanceManager.Application.Models;
using FinanceManager.Application.UseCases;
using Microsoft.AspNetCore.WebUtilities;

public sealed record LoginUserResult(string? LoginLink, IEnumerable<UseCaseError> Errors) : UseCaseResult(Errors);
public sealed class LoginUser(IIdentityService identityService, ILoginTicketStore ticketStore)
{
    public async Task<LoginUserResult> ExecuteAsync(LoginModel model)
    {
        if ((await identityService.FindByEmailAsync(model.Email)) is null)
        {
            return new LoginUserResult(null, [new UseCaseInvalidOperationError("Your email or password is incorrect.")]);
        }
        if (!await identityService.CanSignInAsync(model.Email))
        {
            return new LoginUserResult(null, [new UseCaseInvalidOperationError("You must confirm your email before logging in.")]);
        }

        var (succeeded, isLockedOut) = await identityService.CheckPasswordAsync(model.Email, model.Password);

        if (!succeeded)
        {
            if (isLockedOut)
            {
                return new LoginUserResult(null, [new UseCaseInvalidOperationError("Your account is locked out.")]);
            }

            return new LoginUserResult(null, [new UseCaseInvalidOperationError("Your email or password is incorrect.")]);
        }

        var ticket = ticketStore.Create(model.Email, model.RememberMe);

        var loginLink = QueryHelpers.AddQueryString(Pages.AuthLogin, new Dictionary<string, string?>
        {
            [Parameters.Key] = ticket.ToString(),
            [Parameters.ReturnPath] = model.ReturnPath
        });

        return new LoginUserResult(loginLink, []);
    }
}