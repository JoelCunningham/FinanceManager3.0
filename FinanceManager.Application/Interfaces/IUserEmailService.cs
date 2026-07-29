namespace FinanceManager.Application.Interfaces;

public interface IUserEmailService
{
    Task SendRegistrationConfirmationAsync(string name, string address, string confirmationLink);
    Task SendPasswordResetAsync(string name, string address, string resetLink);
    Task SendMfaCodeAsync(string name, string address, string mfaCode);
    Task SendEmailUpdateConfirmationAsync(string name, string address, string confirmationLink);
}
