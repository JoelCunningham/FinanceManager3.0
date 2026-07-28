namespace FinanceManager.Application.Interfaces;

using FinanceManager.Application.DTOs;

public interface ILoginTicketStore
{
    Guid Create(string email, bool rememberMe);
    bool TryConsume(Guid key, out LoginTicket? ticket);
}