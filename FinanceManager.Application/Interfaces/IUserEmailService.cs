namespace FinanceManager.Application.Interfaces;

public interface IUserEmailService
{
    Task SendEmailConfirmationAsync(string name, string address, string confirmationLink);
    Task SendPasswordResetAsync(string name, string address, string resetLink);
}
