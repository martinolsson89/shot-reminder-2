
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace shot_reminder_2.Infrastructure.Presistence.Mongo.Documents;

public class ShotsInventoryDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]

    public Guid UserId { get; set; }
    public int ShotsLeft { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}
