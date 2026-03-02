using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using shot_reminder_2.Api.Extensions;
using shot_reminder_2.Contracts.Google;
using shot_reminder_2.Infrastructure.Presistence.Mongo.Collections;
using shot_reminder_2.Infrastructure.Presistence.Mongo.Context;
using shot_reminder_2.Infrastructure.Presistence.Mongo.Documents;
using shot_reminder_2.Infrastructure.Presistence.Mongo.Options;

namespace shot_reminder_2.Api.Controllers;

[ApiController]
[Route("api/google")]
public sealed class GoogleCalendarController : ControllerBase
{
    private const string StatePrefix = "google-oauth:";
    private readonly GoogleOAuthOptions _options;
    private readonly IMongoCollection<UserDocument> _users;
    private readonly IMemoryCache _cache;

    public GoogleCalendarController(
        IOptions<GoogleOAuthOptions> options,
        IMongoDbContext context,
        IMemoryCache cache)
    {
        _options = options.Value;
        _users = context.GetCollection<UserDocument>(CollectionNames.Users);
        _cache = cache;
    }

    [Authorize]
    [HttpGet("status")]
    public async Task<ActionResult<GoogleConnectionStatusResponse>> GetStatusAsync(CancellationToken ct)
    {
        var userId = User.GetUserId();
        var user = await _users.Find(x => x.Id == userId).FirstOrDefaultAsync(ct);

        if (user is null)
            return NotFound();

        return Ok(new GoogleConnectionStatusResponse(!string.IsNullOrWhiteSpace(user.GoogleRefreshToken)));
    }

    [Authorize]
    [HttpGet("connect")]
    public ActionResult<GoogleConnectResponse> Connect()
    {
        var userId = User.GetUserId();
        var state = Guid.NewGuid().ToString("N");
        _cache.Set($"{StatePrefix}{state}", userId, TimeSpan.FromMinutes(Math.Max(1, _options.StateCacheMinutes)));

        var request = new GoogleAuthorizationCodeRequestUrl(new Uri(GoogleAuthConsts.AuthorizationUrl))
        {
            ClientId = _options.ClientId,
            RedirectUri = _options.RedirectUri,
            Scope = string.Join(' ', new[] { Google.Apis.Calendar.v3.CalendarService.Scope.CalendarEvents }),
            AccessType = "offline",
            Prompt = "consent",
            State = state
        };

        return Ok(new GoogleConnectResponse(request.Build().ToString()));
    }

    [AllowAnonymous]
    [HttpGet("callback")]
    public async Task<IActionResult> Callback([FromQuery] string? code, [FromQuery] string? state, [FromQuery] string? error, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(error))
            return Redirect(WithStatus("error"));

        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(state))
            return BadRequest("Missing OAuth parameters.");

        if (!_cache.TryGetValue($"{StatePrefix}{state}", out Guid userId))
            return BadRequest("OAuth state expired.");

        var flow = CreateFlow();
        var token = await flow.ExchangeCodeForTokenAsync(userId.ToString(), code, _options.RedirectUri, ct);

        if (string.IsNullOrWhiteSpace(token.RefreshToken))
            return Redirect(WithStatus("missing_refresh"));

        var update = Builders<UserDocument>.Update
            .Set(x => x.GoogleRefreshToken, token.RefreshToken)
            .Set(x => x.GoogleCalendarId, "primary")
            .Unset(x => x.GoogleNextShotEventId);

        await _users.UpdateOneAsync(x => x.Id == userId, update, cancellationToken: ct);

        return Redirect(WithStatus("connected"));
    }

    [Authorize]
    [HttpPost("disconnect")]
    public async Task<IActionResult> DisconnectAsync(CancellationToken ct)
    {
        var userId = User.GetUserId();
        var update = Builders<UserDocument>.Update
            .Unset(x => x.GoogleRefreshToken)
            .Unset(x => x.GoogleNextShotEventId)
            .Set(x => x.GoogleCalendarId, "primary");

        await _users.UpdateOneAsync(x => x.Id == userId, update, cancellationToken: ct);
        return NoContent();
    }

    private GoogleAuthorizationCodeFlow CreateFlow()
        => new(new GoogleAuthorizationCodeFlow.Initializer
        {
            ClientSecrets = new ClientSecrets
            {
                ClientId = _options.ClientId,
                ClientSecret = _options.ClientSecret
            },
            Scopes = new[] { Google.Apis.Calendar.v3.CalendarService.Scope.CalendarEvents }
        });

    private string WithStatus(string status)
        => QueryHelpers.AddQueryString(_options.ClientRedirectUri, "google", status);
}
