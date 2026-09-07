namespace FinanceManager.WebApp.Utilities;

using System.Text;

public class LanguageUtilities
{
    public static string Pluralise(string plural, int count, bool opposite = false)
    {
        if (count != 1 && !opposite || count == 1 && opposite) return plural;

        if (plural == "them")  return count == 1 ? "it" : "them";

        string[] exceptions = ["ies", "oes", "ses", "shes", "ches"];
        var exception = exceptions.FirstOrDefault(e => plural.EndsWith(e, StringComparison.OrdinalIgnoreCase));

        if (exception != null && count == 1)
        {
            return plural[..^exception.Length] + "y";
        }

        return count != 1 && !opposite || count == 1 && opposite ? plural : plural[..^1];
    }
}
