namespace FinanceManager.WebApp.Utilities;

using FinanceManager.Application.Constants;
using FinanceManager.Application.DTOs;
using Havit.Blazor.Components.Web.Bootstrap;
using System.Drawing;
using System.Reflection;

public class CategoryUtilities
{
    public static string GetCategoryName(TransactionSummary transaction)
    {
        return transaction.Category?.Name ?? CategoryConstants.UncategorisedName;
    }

    public static string GetCategoryFullName(TransactionSummary transaction)
    {
        return GetCategoryFullName(transaction.Category);
    }

    public static string GetCategoryFullName(CategorySummary? category)
    {
        return category is null
            ? CategoryConstants.UncategorisedName
            : $"{category.GroupName} - {category.Name}";
    }

    public static BootstrapIcon GetCategroyGroupIcon(string iconName)
    {
        if (!string.IsNullOrWhiteSpace(iconName))
        {
            var prop = typeof(BootstrapIcon).GetProperty(iconName, BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase);
            if (prop?.GetValue(null) is BootstrapIcon icon) return icon;
        }

        return BootstrapIcon.QuestionCircle;
    }

    public static string GetElementStyle(string? baseColour)
    {
        if (baseColour is null) return string.Empty;

        return $"""
            background-color: {baseColour}; 
            color: {GetContrastTextColour(baseColour)}; 
            --background-color: {baseColour};
            --hover-color: {GetHoverColour(baseColour)};
        """;

    }

    private static string GetContrastTextColour(string backgroundColour)
    {
        var value = backgroundColour[1..];
        var r = int.Parse(value[..2], System.Globalization.NumberStyles.HexNumber);
        var g = int.Parse(value.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        var b = int.Parse(value.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);

        var luminance = ((0.299 * r) + (0.587 * g) + (0.114 * b)) / 255;
        return luminance > 0.6 ? ColourConstants.Black : ColourConstants.White;
    }

    private static string GetHoverColour(string hex)
    {
        var c = ColorTranslator.FromHtml(hex);

        double h = c.GetHue() / 360.0;
        double s = c.GetSaturation();
        double l = c.GetBrightness();

        l = l > 0.5 ? l * 0.9 : l * 1.1;

        double q = l < 0.5 ? l * (1 + s) : l + s - l * s;
        double p = 2 * l - q;

        double HueToRgb(double t)
        {
            if (t < 0) t += 1;
            if (t > 1) t -= 1;
            if (t < 1.0 / 6) return p + (q - p) * 6 * t;
            if (t < 0.5) return q;
            if (t < 2.0 / 3) return p + (q - p) * (2.0 / 3 - t) * 6;
            return p;
        }

        int r = (int)(HueToRgb(h + 1.0 / 3) * 255);
        int g = (int)(HueToRgb(h) * 255);
        int b = (int)(HueToRgb(h - 1.0 / 3) * 255);

        return $"#{r:X2}{g:X2}{b:X2}";
    }
}
