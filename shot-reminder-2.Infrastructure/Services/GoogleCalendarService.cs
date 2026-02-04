
using Google.Apis.Calendar.v3.Data;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
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

    public GoogleCalendarService(
        GoogleCalendarClientFactory factory,
        IOptions<GoogleOAuthOptions> options,
        IMongoDbContext context)
    {
        _factory = factory;
        _opt = options.Value;
        _users = context.GetCollection<UserDocument>(CollectionNames.Users);
    }

    public async Task UpsertNextShotEventAsync(Guid userId, DateTime nextDueAtUtc, string leg, CancellationToken ct = default)
    {
        var user = await _users.Find(x => x.Id == userId).FirstOrDefaultAsync(ct);
        if (user is null)
            throw new InvalidOperationException($"User '{userId}' not found.");

        var calendarId = user.GoogleCalendarId ?? _opt.CalendarId;

        var svc = await _factory.CreateAsync(_opt.RefreshToken, ct);

        // Convert UTC -> local time (Stockholm)
        var tz = TimeZoneInfo.FindSystemTimeZoneById(_opt.TimeZoneId);
        var local = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(nextDueAtUtc, DateTimeKind.Utc), tz);
        var ben = leg.ToLower() == "left" ? "höger" : "vänster";


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
            return;
        }

        var created = await svc.Events.Insert(ev, calendarId)
            .ExecuteAsync(ct);

        await _users.UpdateOneAsync(
            x => x.Id == userId,
            Builders<UserDocument>.Update.Set(x => x.GoogleNextShotEventId, created.Id),
            cancellationToken: ct);
    }
}
