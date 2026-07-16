namespace FinanceManager.Application.Configuration;

using FinanceManager.Application.Configuration.Base;

public class TokenConfig : IConfig
{
    public static string SectionName => "Token";

    public int TokenLifespan { get; set; }
}