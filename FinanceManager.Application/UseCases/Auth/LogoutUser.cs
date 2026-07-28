namespace FinanceManager.Application.UseCases.Auth;

using FinanceManager.Application.Constants.Navigation;
using FinanceManager.Application.UseCases;

public sealed record LogoutUserResult(string? LogoutLink, IEnumerable<UseCaseError> Errors) : UseCaseResult(Errors);
public sealed class LogoutUser()
{
    public static async Task<LogoutUserResult> ExecuteAsync()
    {
        return new LogoutUserResult(Pages.AuthLogout, []);
    }
}