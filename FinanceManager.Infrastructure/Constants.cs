namespace FinanceManager.Infrastructure;

public class Constants
{
    public const string MigrationsAssembly = "FinanceManager.Infrastructure";
    public const string ConnectionStringName = "ConnectionStrings__Default";

    public static readonly string[] MLGenericTerms =
    [
        "debit", "credit", "card", "payment", "purchase", "withdraw", "withdrawal", "osko", "aus", "paid", "eftpos", "deposit", "sp", "fee", "bonus"
    ];
}
