namespace FinanceManager.WebApp.Utilities;

using FinanceManager.Application.Constants;
using FinanceManager.Application.Enums;
using Microsoft.JSInterop;

public class ThemeUtilities
{
    private const string DarkString = "dark";
    private const string LightString = "light";
    private const string SystemString = "system";

    public static async Task UpdateTheme(ColourTheme theme, IJSRuntime js)
    {
        var themeString = theme switch
        {
            ColourTheme.Light => LightString,
            ColourTheme.Dark => DarkString,
            ColourTheme.System => SystemString,
            _ => throw new ArgumentOutOfRangeException(nameof(theme), theme, null)
        };

        try
        {
            await js.InvokeVoidAsync(JsCommands.SetTheme, themeString);
        }
        catch { }
    }

    public static async Task<string> GetTextColour(IJSRuntime js)
    {
        try
        {
            var effectiveTheme = await js.InvokeAsync<string>(JsCommands.GetEffectiveTheme);
            return effectiveTheme == DarkString ? ColourConstants.Light : ColourConstants.Black;
        }
        catch
        {
            return ColourConstants.Black;
        }
    }

    public static string GetColourModeDescription(ColourTheme colourMode)
    {
        return colourMode switch
        {
            ColourTheme.Light => "Light mode",
            ColourTheme.Dark => "Dark mode",
            ColourTheme.System => "Use system settings",
            _ => throw new ArgumentOutOfRangeException(nameof(colourMode), colourMode, null)
        };
    }
}
