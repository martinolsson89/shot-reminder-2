
namespace shot_reminder_2.Infrastructure.Presistence.Mongo.Options;

public sealed class GoogleOAuthOptions
{
    public string ClientId { get; init; } = default!;
    public string ClientSecret { get; init; } = default!;
    public string RefreshToken { get; init; } = default!;

    public string CalendarId { get; init; } = "primary";
    public string TimeZoneId { get; init; } = "Europe/Stockholm";
    public string RedirectUri { get; init; } = default!;
    public string ClientRedirectUri { get; init; } = default!;
    public int StateCacheMinutes { get; init; } = 10;
}
