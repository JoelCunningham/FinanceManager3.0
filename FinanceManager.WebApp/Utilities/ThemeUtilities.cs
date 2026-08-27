namespace FinanceManager.WebApp.Utilities;

using FinanceManager.Application.Common;
using FinanceManager.Application.Enums;
using Microsoft.JSInterop;

public class ThemeUtilities
{
    public const string ThemePlaceholder = "ThemePlaceholder";
    private const string DarkString = "dark";
    private const string LightString = "light";

    public static async Task UpdateTheme(ColourTheme preferred, IJSRuntime js, UserState userState)
    {
        var systemString = await js.InvokeAsync<string>(JsCommands.GetSystemTheme);
        var system = systemString == DarkString ? ColourTheme.Dark : ColourTheme.Light;

        var mode = preferred == ColourTheme.System ? system : preferred;

        await js.InvokeVoidAsync(JsCommands.SetTheme, mode == ColourTheme.Dark ? DarkString : LightString);
        userState.UpdateTheme(mode);
    }

    public static string GetTextColour(ColourTheme? colourMode)
    {
        return colourMode == ColourTheme.Dark ? "#d8d8d8" : "#000000";
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
