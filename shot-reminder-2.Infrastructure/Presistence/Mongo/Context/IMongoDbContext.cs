

using MongoDB.Driver;

namespace shot_reminder_2.Infrastructure.Presistence.Mongo.Context;

public interface IMongoDbContext
{
    IMongoDatabase Database { get; }
    IMongoCollection<T> GetCollection<T>(string name);
}
