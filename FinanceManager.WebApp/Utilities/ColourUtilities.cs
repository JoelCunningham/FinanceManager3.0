namespace FinanceManager.WebApp.Utilities;

using Havit.Blazor.Components.Web.Bootstrap;

public class ColourUtilities
{
    public static ThemeColor GetUsageColor(decimal proportion)
    {
        if (proportion > 1.0m) return ThemeColor.Danger;
        if (proportion == 1.0m) return ThemeColor.Primary;
        if (proportion < 1.0m) return ThemeColor.Success;
        return ThemeColor.Secondary;
    }
}
