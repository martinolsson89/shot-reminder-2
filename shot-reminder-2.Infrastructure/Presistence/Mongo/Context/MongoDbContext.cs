
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using shot_reminder_2.Infrastructure.Presistence.Mongo.Options;


namespace shot_reminder_2.Infrastructure.Presistence.Mongo.Context;

public class MongoDbContext : IMongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IOptions<MongoOptions> options)
    {
        var opt = options.Value;

        if (string.IsNullOrWhiteSpace(opt.ConnectionString))
            throw new InvalidOperationException("Mongo connection string is missing(Mongo: ConnectionString).");

        if (string.IsNullOrEmpty(opt.DatabaseName))
            throw new InvalidOperationException("Mongo database name is missing (Mongo:DatabaseName).");

        var client = new MongoClient(opt.ConnectionString);
        _database = client.GetDatabase(opt.DatabaseName);
    }

    public IMongoDatabase Database => _database;

    public IMongoCollection<T> GetCollection<T>(string name)
        => _database.GetCollection<T>(name);

}
