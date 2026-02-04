
namespace shot_reminder_2.Infrastructure.Presistence.Mongo.Options;

public sealed class MongoOptions
{
    public const string SectionName = "Mongo";

    public string ConnectionString { get; init; } = default!;
    public string DatabaseName { get; init; } = default!;
}
