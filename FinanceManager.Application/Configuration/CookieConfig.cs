namespace FinanceManager.Application.Configuration;

using FinanceManager.Application.Configuration.Base;

public class CookieConfig : IConfig
{
    public static string SectionName => "Cookie";

    public string LoginPath { get; set; } = default!;
    public string LogoutPath { get; set; } = default!;
    public int ExpireDays { get; set; }
    public bool SlidingExpiration { get; set; }
    public bool HttpOnly { get; set; }
}