namespace FinanceManager.Application.Configuration;

using FinanceManager.Application.Configuration.Base;

public class EmailConfig : IConfig
{
    public static string SectionName => "Email";

    public string SenderName { get; set; } = default!;
    public string SenderEmail { get; set; } = default!;

    public SmtpConfig Smtp { get; set; } = new();
}

public class SmtpConfig
{
    public string Host { get; set; } = default!;
    public int Port { get; set; }
    public bool EnableSsl { get; set; }
    public string Username { get; set; } = default!;
    public string Password { get; set; } = default!;
}