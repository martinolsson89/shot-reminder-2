
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace shot_reminder_2.Infrastructure.Presistence.Mongo.Documents;

public sealed class TakenShotDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }

    [BsonRepresentation(BsonType.String)]
    public Guid UserId { get; set; }
    public DateTime TakenAtUtc { get; set; }
    public enLeg Leg { get; set; }
    public string? Comment { get; set; }
}
