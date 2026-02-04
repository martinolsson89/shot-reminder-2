using System.Security.Claims;

namespace shot_reminder_2.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(value))
            throw new UnauthorizedAccessException("Missing user id claim.");

        return Guid.Parse(value);
    }
}