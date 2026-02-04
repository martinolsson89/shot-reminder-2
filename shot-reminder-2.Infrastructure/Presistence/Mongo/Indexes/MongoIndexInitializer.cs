
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using shot_reminder_2.Application.Interfaces;
using shot_reminder_2.Infrastructure.Presistence.Mongo.Collections;
using shot_reminder_2.Infrastructure.Presistence.Mongo.Context;
using shot_reminder_2.Infrastructure.Presistence.Mongo.Documents;

namespace shot_reminder_2.Infrastructure.Presistence.Mongo.Indexes;

public sealed class MongoIndexInitializer : IMongoIndexInitializer
{
    private readonly IMongoDbContext _context;
    private readonly ILogger<MongoIndexInitializer> _logger;

    public MongoIndexInitializer(IMongoDbContext context, ILogger<MongoIndexInitializer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task EnsureIndexesAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Ensuring MongoDB indexes...");

        await EnsureUserIndexesAsync(ct);
        await EnsureTakenShotIndexesAsync(ct);
        // Add more as you implement those collections:
        // await EnsureUpcomingShotIndexesAsync(ct);
        // await EnsureInventoryIndexesAsync(ct);

        _logger.LogInformation("MongoDB indexes ensured.");
    }

    private async Task EnsureUserIndexesAsync(CancellationToken ct)
    {
        var users = _context.GetCollection<UserDocument>(CollectionNames.Users);

        var emailUnique = new CreateIndexModel<UserDocument>(
            Builders<UserDocument>.IndexKeys.Ascending(x => x.Email),
            new CreateIndexOptions
            {
                Name = "ux_users_email",
                Unique = true,
                Collation = new Collation(locale: "en", strength: CollationStrength.Secondary)
            });

        await users.Indexes.CreateOneAsync(emailUnique, cancellationToken: ct);
    }

    private async Task EnsureTakenShotIndexesAsync(CancellationToken ct)
    {
        var shots = _context.GetCollection<TakenShotDocument>(CollectionNames.TakenShots);

        // Fast "get shots for user sorted by date desc" and "get latest shot for user"
        var byUserAndTakenAtDesc = new CreateIndexModel<TakenShotDocument>(
            Builders<TakenShotDocument>.IndexKeys
                .Ascending(x => x.UserId)
                .Descending(x => x.TakenAtUtc),
            new CreateIndexOptions
            {
                Name = "ix_taken_shots_user_takenAt_desc"
            });

        await shots.Indexes.CreateOneAsync(byUserAndTakenAtDesc, cancellationToken: ct);
    }

}
