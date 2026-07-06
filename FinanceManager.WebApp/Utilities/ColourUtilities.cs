namespace FinanceManager.WebApp.Utilities;

public class ColourUtilities
{
    private const decimal _generousThreshold = 0.005m;

    public const string PrimaryColour = "var(--colour-primary)";
    public const string SecondaryColour = "var(--colour-secondary)";
    public const string SuccessColour = "var(--colour-success)";
    public const string WarningColour = "var(--colour-warning)";
    public const string DangerColour = "var(--colour-danger)";
    public const string BlackColour = "var(--colour-black)";

    public static string GetUsageStyle(decimal? proportion, bool generous = false, bool isIncome = false)
    {
        var offset = generous ? _generousThreshold : 0.0m;

        if (proportion is null) return "none";
        if (proportion > 1.0m + offset) return isIncome ? "success" : "danger";
        if (proportion < 1.0m - offset) return isIncome ? "danger" : "success";
        return "normal";
    }

    public static string GetUsageColour(decimal? proportion, bool generous = false, bool isIncome = false)
    {
        var offset = generous ? _generousThreshold : 0.0m;
        if (proportion is null) return BlackColour;
        if (proportion > 1.0m + offset) return isIncome ? SuccessColour : DangerColour;
        if (proportion < 1.0m - offset) return isIncome ? DangerColour : SuccessColour;
        return SecondaryColour;
    }

    public static string GetAmountColour(decimal amount)
    {
        if (amount > 0) return SuccessColour;
        if (amount < 0) return DangerColour;
        return PrimaryColour;
    }
}
