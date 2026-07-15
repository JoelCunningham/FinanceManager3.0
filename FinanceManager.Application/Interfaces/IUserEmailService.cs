namespace FinanceManager.Application.Interfaces;

public interface IUserEmailService
{
    Task SendEmailAsync(string email, string subject, string htmlMessage);
}
