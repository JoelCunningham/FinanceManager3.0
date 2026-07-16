namespace FinanceManager.WebApp.Authentication;

using FinanceManager.WebApp.Navigation;
using Microsoft.AspNetCore.WebUtilities;

public sealed class AuthenticationService(PendingLoginStore PendingLoginStore)
{
    public string GetLoginUrl(string email, string password, bool rememberMe, string? returnPath)
    {
        var key = PendingLoginStore.Add(email, password, rememberMe);

        var loginUrl = QueryHelpers.AddQueryString(Pages.AuthLogin, new Dictionary<string, string?>
        {
            [Parameters.Key] = key.ToString(),
            [Parameters.ReturnPath] = returnPath
        });

        return loginUrl;
    }

    public string GetLogoutUrl()
    {
        return Pages.AuthLogout;
    }
}