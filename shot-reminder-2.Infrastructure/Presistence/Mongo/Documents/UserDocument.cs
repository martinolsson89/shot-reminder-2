using MongoDB.Bson.Serialization.Attributes;

namespace shot_reminder_2.Infrastructure.Presistence.Mongo.Documents;

public class UserDocument
{
    [BsonId]
    [BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public Guid Id { get; set; }

    public string Email { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime CreatedAtUtc { get; set; }

    public string? GoogleCalendarId { get; set; } = "primary";
    public string? GoogleNextShotEventId { get; set; }
    public string? GoogleRefreshToken { get; set; }
}
