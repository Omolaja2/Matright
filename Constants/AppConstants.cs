namespace PharMarket.Constants;

public static class AppConstants
{
    public const string CurrencySymbol = "\u20a6";
    public const string CompanyName = "PharMarket";
    public const int DefaultPageSize = 20;
    public const int LowStockThreshold = 10;
    public const int ExpiryAlertDays = 30;
}

public static class SessionKeys
{
    public const string CartItems = "CartItems";
    public const string CurrentCashier = "CurrentCashier";
}

public static class ClaimTypes
{
    public const string UserId = "UserId";
    public const string UserName = "UserName";
    public const string Role = "Role";
}
