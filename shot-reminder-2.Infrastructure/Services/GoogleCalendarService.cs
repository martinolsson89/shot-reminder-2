using Google;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Calendar.v3.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using shot_reminder_2.Application.Commons;
using shot_reminder_2.Application.Interfaces;
using shot_reminder_2.Infrastructure.Presistence.Mongo.Collections;
using shot_reminder_2.Infrastructure.Presistence.Mongo.Context;
using shot_reminder_2.Infrastructure.Presistence.Mongo.Documents;
using shot_reminder_2.Infrastructure.Presistence.Mongo.Options;

namespace shot_reminder_2.Infrastructure.Services;

public sealed class GoogleCalendarService : ICalendarService
{
    private readonly GoogleCalendarClientFactory _factory;
    private readonly GoogleOAuthOptions _opt;
    private readonly IMongoCollection<UserDocument> _users;
    private readonly ILogger<GoogleCalendarService> _logger;

    public GoogleCalendarService(
        GoogleCalendarClientFactory factory,
        IOptions<GoogleOAuthOptions> options,
        IMongoDbContext context,
        ILogger<GoogleCalendarService> logger)
    {
        _factory = factory;
        _opt = options.Value;
        _users = context.GetCollection<UserDocument>(CollectionNames.Users);
        _logger = logger;
    }

    public async Task UpsertNextShotEventAsync(Guid userId, DateTime nextDueAtUtc, string leg, CancellationToken ct = default)
    {
        string? calendarId = null;

        try
        {
            var user = await _users.Find(x => x.Id == userId).FirstOrDefaultAsync(ct);
            if (user is null)
                throw new NotFoundException($"User '{userId}' not found.");

            calendarId = user.GoogleCalendarId ?? _opt.CalendarId;

            var svc = await _factory.CreateAsync(_opt.RefreshToken, ct);

            var tz = TimeZoneInfo.FindSystemTimeZoneById(_opt.TimeZoneId);
            var local = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(nextDueAtUtc, DateTimeKind.Utc), tz);
            var ben = leg.Equals("left", StringComparison.OrdinalIgnoreCase) ? "vänster" : "höger";

            var ev = new Event
            {
                Summary = "Ta spruta",
                Description = $"Dags att ta sprutan i {ben} ben",
                Start = new EventDateTime { DateTimeDateTimeOffset = local, TimeZone = _opt.TimeZoneId },
                End = new EventDateTime { DateTimeDateTimeOffset = local.AddMinutes(10), TimeZone = _opt.TimeZoneId }
            };

            if (!string.IsNullOrWhiteSpace(user.GoogleNextShotEventId))
            {
                await svc.Events.Update(ev, calendarId, user.GoogleNextShotEventId)
                    .ExecuteAsync(ct);

                _logger.LogInformation(
                    "Updated next-shot event for user {UserId} in calendar {CalendarId}.",
                    userId,
                    calendarId);

                return;
            }

            var created = await svc.Events.Insert(ev, calendarId)
                .ExecuteAsync(ct);

            await _users.UpdateOneAsync(
                x => x.Id == userId,
                Builders<UserDocument>.Update.Set(x => x.GoogleNextShotEventId, created.Id),
                cancellationToken: ct);

            _logger.LogInformation(
                "Created next-shot event {EventId} for user {UserId} in calendar {CalendarId}.",
                created.Id,
                userId,
                calendarId);
        }
        catch (TokenResponseException ex)
        {
            _logger.LogError(
                ex,
                "Google token refresh failed for user {UserId}. OAuthError={OAuthError}; OAuthErrorDescription={OAuthErrorDescription}; CalendarId={CalendarId}; TimeZoneId={TimeZoneId}",
                userId,
                ex.Error?.Error,
                ex.Error?.ErrorDescription,
                calendarId ?? _opt.CalendarId,
                _opt.TimeZoneId);
            throw;
        }
        catch (GoogleApiException ex)
        {
            var reasons = ex.Error?.Errors is null
                ? null
                : string.Join(", ", ex.Error.Errors.Select(e => $"{e.Reason}:{e.Message}"));

            _logger.LogError(
                ex,
                "Google Calendar API failed for user {UserId}. HttpStatusCode={HttpStatusCode}; GoogleCode={GoogleCode}; GoogleMessage={GoogleMessage}; GoogleReasons={GoogleReasons}; CalendarId={CalendarId}; TimeZoneId={TimeZoneId}",
                userId,
                ex.HttpStatusCode,
                ex.Error?.Code,
                ex.Error?.Message,
                reasons,
                calendarId ?? _opt.CalendarId,
                _opt.TimeZoneId);
            throw;
        }
        catch (TimeZoneNotFoundException ex)
        {
            _logger.LogError(
                ex,
                "Timezone resolution failed for user {UserId}. TimeZoneId={TimeZoneId}; CalendarId={CalendarId}",
                userId,
                _opt.TimeZoneId,
                calendarId ?? _opt.CalendarId);
            throw;
        }
        catch (InvalidTimeZoneException ex)
        {
            _logger.LogError(
                ex,
                "Timezone data invalid for user {UserId}. TimeZoneId={TimeZoneId}; CalendarId={CalendarId}",
                userId,
                _opt.TimeZoneId,
                calendarId ?? _opt.CalendarId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected Google Calendar failure for user {UserId}. CalendarId={CalendarId}; TimeZoneId={TimeZoneId}",
                userId,
                calendarId ?? _opt.CalendarId,
                _opt.TimeZoneId);
            throw;
        }
    }
}
