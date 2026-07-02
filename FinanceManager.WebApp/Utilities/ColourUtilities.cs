namespace FinanceManager.WebApp.Utilities;

public class ColourUtilities
{
    public static string GetUsageStyle(decimal proportion, bool generous = false)
    {
        if (generous)
        {
            return proportion switch
            {
                > 1.005m => "danger",
                < 0.995m => "success",
                _ => "normal",
            };
        }

        return proportion switch
        {
            > 1.0m => "danger",
            1.0m => "normal",
            < 1.0m => "success"
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

    public const string PrimaryColour = "var(--colour-primary)";
    public const string SuccessColour = "var(--colour-success)";
    public const string WarningColour = "var(--colour-warning)";
    public const string DangerColour = "var(--colour-danger)";
}
