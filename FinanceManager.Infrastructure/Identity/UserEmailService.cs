namespace FinanceManager.Infrastructure.Identity;

using FinanceManager.Application.Interfaces;

public class UserEmailService : IUserEmailService
{
    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        Console.WriteLine("========== EMAIL ==========");
        Console.WriteLine($"To: {email}");
        Console.WriteLine($"Subject: {subject}");
        Console.WriteLine("Message:");
        Console.WriteLine(htmlMessage);
        Console.WriteLine("===========================");

        return Task.CompletedTask;
    }
}
