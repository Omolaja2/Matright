using System.Security.Claims;

namespace PharMarket.Helpers;

public static class UserExtensions
{
    public static int? GetStoreId(this ClaimsPrincipal user)
    {
        var storeIdClaim = user.FindFirst("StoreId");
        if (storeIdClaim != null && int.TryParse(storeIdClaim.Value, out var storeId))
            return storeId;
        return null;
    }

    public static int GetUserId(this ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
            return userId;
        return 0;
    }

    public static string GetUserName(this ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
    }

    public static string GetUserRole(this ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.Role)?.Value ?? "Apprentice";
    }

    public static bool IsAdmin(this ClaimsPrincipal user)
    {
        return user.GetUserRole() == "Admin";
    }
}
