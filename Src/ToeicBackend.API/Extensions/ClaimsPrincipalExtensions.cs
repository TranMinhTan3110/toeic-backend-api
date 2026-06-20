using System.Security.Claims;

namespace ToeicBackend.API.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static string? GetFirebaseUserId(this ClaimsPrincipal user)
    {
        return user.FindFirstValue("user_id")
            ?? user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? user.FindFirstValue("sub");
    }
}
