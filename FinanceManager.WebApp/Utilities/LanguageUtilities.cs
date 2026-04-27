namespace FinanceManager.WebApp.Utilities;

public class LanguageUtilities
{
    public static string Pluralise(string plural, int count)
    {
        if (count != 1) return plural;

        string[] exceptions = ["ies", "oes", "ses", "shes", "ches"];
        var exception = exceptions.FirstOrDefault(e => plural.EndsWith(e, StringComparison.OrdinalIgnoreCase));

        if (exception != null && count == 1)
        {
            return plural[..^exception.Length] + "y";
        }

        return count != 1 ? plural : plural[..^1];
    }
}
