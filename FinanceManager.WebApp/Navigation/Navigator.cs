namespace FinanceManager.WebApp.Navigation;

using System.Text;

public class Navigator
{
    public static string CreateUrl(string? page, Dictionary<string, string?>? parameters)
    {
        StringBuilder urlBuilder = new();

        urlBuilder.Append(page ?? Pages.Root);

        if (parameters != null)
        {
            urlBuilder.Append(BeginParameters);
            foreach (var (name, value) in parameters)
            {
                if (string.IsNullOrWhiteSpace(value) || value == Pages.Root) continue;
                urlBuilder.Append($"{name}{AssignParameter}{Uri.EscapeDataString(value)}");
                urlBuilder.Append(JoinParameters);
            }
            urlBuilder.Length--;
        }

        return urlBuilder.ToString();
    }

    private const char BeginParameters = '?';
    private const char AssignParameter = '=';
    private const char JoinParameters = '&';
}
