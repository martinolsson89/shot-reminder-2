
namespace shot_reminder_2.Infrastructure.Presistence.Mongo.Options;

public sealed class JwtOptions
{
    public string Key { get; init; } = default!;
    public string Issuer { get; init; } = default!;
    public string Audience { get; init; } = default!;
    public int AccessTokenMinutes { get; init; } = 15;
}
