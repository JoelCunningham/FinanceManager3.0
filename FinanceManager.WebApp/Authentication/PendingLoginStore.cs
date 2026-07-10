namespace FinanceManager.WebApp.Authentication;

using System.Collections.Concurrent;

public sealed class PendingLoginStore
{
    private readonly ConcurrentDictionary<Guid, PendingLogin> Logins = new();

    public Guid Add(string email, string password, bool rememberMe)
    {
        var key = Guid.NewGuid();
        Logins[key] = new PendingLogin(email, password, rememberMe);
        return key;
    }

    public bool TryRemove(Guid key, out PendingLogin? login)
    {
        return Logins.TryRemove(key, out login);
    }
}

public sealed record PendingLogin(string Email, string Password, bool RememberMe);