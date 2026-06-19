namespace FinanceManager.WebApp.Utilities;

using Havit.Blazor.Components.Web.Bootstrap;

public class ColourUtilities
{
    public static ThemeColor GetUsageColor(decimal proportion)
    {
        return proportion switch
        {
            > 1.0m => ThemeColor.Danger,
            1.0m => ThemeColor.Primary,
            < 1.0m => ThemeColor.Success
        };
    }

    public static string GetAmountColour(decimal amount)
    {
        return amount switch
        {
            > 0 => SuccessColour,
            < 0 => DangerColour,
            _ => PrimaryColour
        };
    }

    public const string PrimaryColour = "var(--bs-primary)";
    public const string SuccessColour = "var(--bs-success)";
    public const string WarningColour = "var(--bs-warning)";
    public const string DangerColour = "var(--bs-danger)";
}
