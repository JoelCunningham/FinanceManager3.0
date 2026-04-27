namespace FinanceManager.WebApp.Utilities;

using FinanceManager.Application.DTOs;

public class CategoryUtilities
{
    public static string GetCategoryName(TransactionSummary transaction)
    {
        return transaction.Category?.Name ?? "Uncategorised";
    }

    public static string GetCategoryFullName(TransactionSummary transaction)
    {
        return transaction.Category is null
            ? "Uncategorised"
            : $"{transaction.Category.GroupName} - {transaction.Category.Name}";
    }

    public static string GetCategoryStyle(TransactionSummary transaction)
    {
        if (transaction.Category is null) return string.Empty;

        var backgroundColour = transaction.Category.GroupColour;
        return $"background-color: {backgroundColour}; color: {GetContrastTextColour(backgroundColour)};";
    }

    private static string GetContrastTextColour(string backgroundColour)
    {
        var value = backgroundColour[1..];
        var r = int.Parse(value[..2], System.Globalization.NumberStyles.HexNumber);
        var g = int.Parse(value.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        var b = int.Parse(value.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);

        var luminance = ((0.299 * r) + (0.587 * g) + (0.114 * b)) / 255;
        return luminance > 0.6 ? "#000000" : "#FFFFFF";
    }
}
