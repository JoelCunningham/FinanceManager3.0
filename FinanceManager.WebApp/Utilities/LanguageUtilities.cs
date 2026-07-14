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

    public static string Capitalise(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        return char.ToUpper(input[0]) + input[1..];
    }

    public static string AddSpacesToSentence(string text, bool preserveAcronyms)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;

        StringBuilder newText = new(text.Length * 2);
        newText.Append(text[0]);

        for (int i = 1; i < text.Length; i++)
        {
            if (char.IsUpper(text[i]))
            {
                if ((text[i - 1] != ' ' && !char.IsUpper(text[i - 1])) || (preserveAcronyms && char.IsUpper(text[i - 1]) && i < text.Length - 1 && !char.IsUpper(text[i + 1])))
                {
                    newText.Append(' ');
                }
            }
            newText.Append(text[i]);
        }

        return newText.ToString();
    }

}
