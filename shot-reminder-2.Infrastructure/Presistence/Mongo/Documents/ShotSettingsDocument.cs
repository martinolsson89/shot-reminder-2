using MongoDB.Bson.Serialization.Attributes;

namespace shot_reminder_2.Infrastructure.Presistence.Mongo.Documents;

public sealed class ShotSettingsDocument
{
    [BsonId]
    public string Id { get; set; } = "global";
    public int IntervalDays { get; set; }
    public int LowStockThreshold { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}
