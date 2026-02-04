
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Calendar.v3;
using Google.Apis.Services;
using Microsoft.Extensions.Options;
using shot_reminder_2.Infrastructure.Presistence.Mongo.Options;

namespace shot_reminder_2.Infrastructure.Services;

public sealed class GoogleCalendarClientFactory
{
    private readonly GoogleOAuthOptions _opt;

    public GoogleCalendarClientFactory(IOptions<GoogleOAuthOptions> opt)
        => _opt = opt.Value;

    public async Task<CalendarService> CreateAsync(string refreshToken, CancellationToken ct)
    {
        var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
        {
            ClientSecrets = new ClientSecrets
            {
                ClientId = _opt.ClientId,
                ClientSecret = _opt.ClientSecret
            },
            Scopes = new[] { CalendarService.Scope.CalendarEvents }
        });

        var credential = new UserCredential(flow, "me", new TokenResponse { RefreshToken = refreshToken });

        // Ensure we have a valid access token
        await credential.RefreshTokenAsync(ct);

        return new CalendarService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "shot-reminder-2"
        });
    }

}
