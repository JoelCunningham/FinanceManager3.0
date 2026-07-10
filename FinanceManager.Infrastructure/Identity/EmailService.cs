namespace FinanceManager.Infrastructure.Identity;

using Microsoft.AspNetCore.Identity.UI.Services;

public class EmailService : IEmailSender
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
