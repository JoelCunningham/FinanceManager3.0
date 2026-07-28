namespace FinanceManager.Infrastructure.Identity;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Interfaces;

public class LoginTicketStore : ILoginTicketStore
{
    private readonly Dictionary<Guid, LoginTicket> Store = [];

    public Guid Create(string email, bool rememberMe)
    {
        var key = Guid.NewGuid();
        Store[key] = new LoginTicket { Email = email, RememberMe = rememberMe };

        return key;
    }

    public bool TryConsume(Guid key, out LoginTicket? ticket)
    {
        if (Store.TryGetValue(key, out ticket))
        {
            Store.Remove(key);
            return true;
        }

        return false;
    }
}