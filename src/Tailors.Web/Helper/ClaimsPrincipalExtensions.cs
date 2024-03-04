using System.Security.Claims;

namespace Tailors.Web.Helper;

public static class ClaimsPrincipalExtensions
{
    public const string UrnTailorsAppUserId = "urn:tailors:appuserid";

    public static string GetId(this ClaimsPrincipal user)
    {
        var appUserId = user.FindFirstValue(UrnTailorsAppUserId);
        if (appUserId is null)
            throw new InvalidOperationException($"{UrnTailorsAppUserId} claim not found");
        return appUserId;
    }
}
