using System.Globalization;

namespace PharMarket.Helpers;

public static class CurrencyHelper
{
    private static readonly CultureInfo NigerianCulture = new("en-NG");

    public static string FormatCurrency(decimal amount)
    {
        return amount.ToString("C", NigerianCulture);
    }

    public static string FormatNumber(decimal number)
    {
        return number.ToString("N2", CultureInfo.InvariantCulture);
    }

    public static string FormatPercentage(decimal percentage)
    {
        return $"{percentage:F1}%";
    }

    public static string FormatDate(DateTime date, string format = "dd MMM yyyy")
    {
        return date.ToString(format, CultureInfo.InvariantCulture);
    }

    public static string FormatDateTime(DateTime date)
    {
        return date.ToString("dd MMM yyyy hh:mm tt", CultureInfo.InvariantCulture);
    }
}
