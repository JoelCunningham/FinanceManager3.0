namespace FinanceManager.Infrastructure.Identity;

using FinanceManager.Application.Configuration;
using FinanceManager.Application.Interfaces;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;

public class UserEmailService(IOptions<EmailConfig> options) : IUserEmailService
{
    private readonly EmailConfig Config = options.Value;

    public async Task SendEmailConfirmationAsync(string name, string address, string confirmationLink)
    {
        var subject = "Confirm your email";
        var htmlMessage = $"" +
            $"<p>Hi {name},</p>" +
            $"<p>Please confirm your email by clicking the link below:</p><p><a href='{confirmationLink}'>Confirm Email</a></p>" +
            $"<p>If you did not create an account, please ignore this email.</p>";
        await SendEmailAsync(name, address, subject, htmlMessage);
    }

    public async Task SendPasswordResetAsync(string name, string address, string resetLink)
    {
        var subject = "Reset your password";
        var htmlMessage = $"" +
            $"<p>Hi {name},</p>" +
            $"<p>We received a request to reset your password. You can reset your password by clicking the link below:</p>" +
            $"<p><a href='{resetLink}'>Reset Password</a></p>" +
            $"<p>If you did not request this, please ignore this email.</p>";
        await SendEmailAsync(name, address, subject, htmlMessage);
    }

    private async Task SendEmailAsync(string name, string address, string subject, string htmlMessage)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(Config.SenderName, Config.SenderEmail));
        message.To.Add(new MailboxAddress(name, address));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = htmlMessage };

        using var client = new SmtpClient();
        await client.ConnectAsync(Config.Smtp.Host, Config.Smtp.Port, Config.Smtp.EnableSsl);
        await client.AuthenticateAsync(Config.Smtp.Username, Config.Smtp.Password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
