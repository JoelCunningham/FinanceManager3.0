namespace FinanceManager.WebApp.Configuration;

using FinanceManager.WebApp.Configuration.Base;

public class TokenConfig : IConfig
{
    public static string SectionName => "Token";

    public int TokenLifespan { get; set; }
}