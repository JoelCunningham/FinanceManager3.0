namespace FinanceManager.Application.Utilities;

using System.Globalization;

public class CurrencyUtilities
{
    public static string FormatCurrency(decimal amount, bool absolute = false)
    {
        if (absolute) amount = Math.Abs(amount);
        return (amount > 0 ? "+" : "") + amount.ToString("C", CultureInfo.CurrentCulture);
    }

    public static decimal? GetProportion(decimal budget, decimal actual)
    {
        if (budget == 0 && actual == 0) return null;
        if (budget == 0 && actual != 0) return decimal.MaxValue;

        return actual / budget;
    }

    public static string GetProportionString(decimal? ProportionValue)
    {
        return ProportionValue == decimal.MaxValue ? "∞ %" : ProportionValue?.ToString("P0", CultureInfo.CurrentCulture) ?? "0 %";
    }

    public static string GetProportionDescription(decimal? ProportionValue, bool IsIncome)
    {
        return ProportionValue switch
        {
            null => "No budget set",
            < 0 => "No budget set",
            0 => "No budget used",
            > 1 => IsIncome ? "Above budget" : "Over budget",
            _ => IsIncome ? "Under budget" : "Within budget"
        };
    }
}
