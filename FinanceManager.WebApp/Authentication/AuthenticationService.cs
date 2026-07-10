namespace FinanceManager.WebApp.Authentication;

using FinanceManager.WebApp.Navigation;

public sealed class AuthenticationService(PendingLoginStore pendingLogins)
{
    public string GetLoginUrl(string email, string password, bool rememberMe, string? returnPath)
    {
        var key = pendingLogins.Add(email, password, rememberMe);
        var parameters = new Dictionary<string, string?>
        {
            [Parameters.Key] = key.ToString(),
            [Parameters.ReturnPath] = returnPath,
        };

        return Navigator.CreateUrl(Pages.AuthLogin, parameters);
    }

    public string GetLogoutUrl()
    {
        return Pages.AuthLogout;
    }
}