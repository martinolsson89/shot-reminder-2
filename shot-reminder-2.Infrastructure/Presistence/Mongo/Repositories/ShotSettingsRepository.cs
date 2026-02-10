using Microsoft.Extensions.Options;
using MongoDB.Driver;
using shot_reminder_2.Application.Interfaces;
using shot_reminder_2.Application.Options;
using shot_reminder_2.Infrastructure.Presistence.Mongo.Collections;
using shot_reminder_2.Infrastructure.Presistence.Mongo.Context;
using shot_reminder_2.Infrastructure.Presistence.Mongo.Documents;

namespace shot_reminder_2.Infrastructure.Presistence.Mongo.Repositories;

public sealed class ShotSettingsRepository : IShotSettingsRepository
{
    private const string GlobalSettingsId = "global";

    private readonly IMongoCollection<ShotSettingsDocument> _settings;
    private readonly IOptions<ShotSettings> _defaults;

    public ShotSettingsRepository(IMongoDbContext context, IOptions<ShotSettings> defaults)
    {
        _settings = context.GetCollection<ShotSettingsDocument>(CollectionNames.ShotSettings);
        _defaults = defaults;
    }

    public async Task<ShotSettings> GetAsync(CancellationToken ct = default)
    {
        var filter = Builders<ShotSettingsDocument>.Filter.Eq(x => x.Id, GlobalSettingsId);
        var doc = await _settings.Find(filter).FirstOrDefaultAsync(ct);

        if (doc is not null)
            return MapToOptions(doc);

        var seed = new ShotSettingsDocument
        {
            Id = GlobalSettingsId,
            IntervalDays = _defaults.Value.IntervalDays,
            LowStockThreshold = _defaults.Value.LowStockThreshold,
            UpdatedAtUtc = DateTime.UtcNow
        };

        try
        {
            await _settings.InsertOneAsync(seed, cancellationToken: ct);
            return MapToOptions(seed);
        }
        catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            var existing = await _settings.Find(filter).FirstOrDefaultAsync(ct);
            return existing is null ? new ShotSettings() : MapToOptions(existing);
        }
    }

    public async Task UpdateAsync(ShotSettings settings, CancellationToken ct = default)
    {
        var filter = Builders<ShotSettingsDocument>.Filter.Eq(x => x.Id, GlobalSettingsId);
        var update = Builders<ShotSettingsDocument>.Update
            .Set(x => x.IntervalDays, settings.IntervalDays)
            .Set(x => x.LowStockThreshold, settings.LowStockThreshold)
            .Set(x => x.UpdatedAtUtc, DateTime.UtcNow);

        await _settings.UpdateOneAsync(
            filter,
            update,
            new UpdateOptions { IsUpsert = true },
            ct);
    }

    private static ShotSettings MapToOptions(ShotSettingsDocument doc)
        => new()
        {
            IntervalDays = doc.IntervalDays,
            LowStockThreshold = doc.LowStockThreshold
        };
}
