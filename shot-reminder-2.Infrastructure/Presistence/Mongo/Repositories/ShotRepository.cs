using MongoDB.Driver;
using shot_reminder_2.Application.Interfaces;
using shot_reminder_2.Domain.Entities;
using shot_reminder_2.Infrastructure.Presistence.Mongo.Collections;
using shot_reminder_2.Infrastructure.Presistence.Mongo.Context;
using shot_reminder_2.Infrastructure.Presistence.Mongo.Documents;

namespace shot_reminder_2.Infrastructure.Presistence.Mongo.Repositories;

public sealed class ShotRepository : IShotRepository
{
    private readonly IMongoCollection<TakenShotDocument> _shots;

    public ShotRepository(IMongoDbContext context)
    {
        _shots = context.GetCollection<TakenShotDocument>(CollectionNames.TakenShots);
    }


    public async Task<IReadOnlyList<TakenShot>> GetAsync(Guid userId, CancellationToken ct = default)
    {
        var filter = Builders<TakenShotDocument>.Filter.Eq(x => x.UserId, userId);

        var shots = await _shots.Find(filter)
            .SortByDescending(x => x.TakenAtUtc)
            .ToListAsync(ct);

        var response = new List<TakenShot>();

        foreach(var shot in shots)
        {
            var s = new TakenShot
                (id: shot.Id, 
                userid: shot.UserId, 
                takenAtUtc: shot.TakenAtUtc, 
                leg: shot.Leg, 
                comment: shot.Comment
                );
            response.Add(s);
        }

        return response;
    }

    public async Task<Guid> InsertAsync(TakenShot shot, CancellationToken ct = default)
    {
        var doc = new TakenShotDocument
        {
            Id = shot.Id,
            UserId = shot.UserId,
            TakenAtUtc = shot.TakenAtUtc,
            Leg = shot.Leg,
            Comment = shot.Comment
        };

        await _shots.InsertOneAsync(doc, cancellationToken: ct);
        return doc.Id;
    }

    public async Task UpdateAsync(TakenShot shot, CancellationToken ct = default)
    {
        var doc = new TakenShotDocument
        {
            Id = shot.Id,
            UserId = shot.UserId,
            TakenAtUtc = shot.TakenAtUtc,
            Leg = shot.Leg,
            Comment = shot.Comment
        };

        var filter = Builders<TakenShotDocument>.Filter.Eq(x => x.Id, shot.Id);
       
        var result = await _shots.ReplaceOneAsync(filter, doc, cancellationToken: ct);

        if (result.MatchedCount == 0)
            throw new KeyNotFoundException();
    }
    public async Task DeleteAsync(Guid shotId, Guid userId, CancellationToken ct = default)
    {
        var filter = Builders<TakenShotDocument>.Filter.And(
            Builders<TakenShotDocument>.Filter.Eq(x => x.Id, shotId),
            Builders<TakenShotDocument>.Filter.Eq(x => x.UserId, userId)
        );

        var result = await _shots.DeleteOneAsync(filter, cancellationToken: ct);

        if (result.DeletedCount == 0)
            throw new KeyNotFoundException();

    }

    public async Task<TakenShot?> GetByIdAsync(Guid shotId, Guid userId, CancellationToken ct = default)
    {
        var filter = Builders<TakenShotDocument>.Filter.And(
            Builders<TakenShotDocument>.Filter.Eq(x => x.Id, shotId),
            Builders<TakenShotDocument>.Filter.Eq(x => x.UserId, userId)
        );

        var doc = await _shots.Find(filter).FirstOrDefaultAsync(ct);

        if (doc is null)
            return null;

        return new TakenShot(
            id: doc.Id,
            userid: doc.UserId,
            takenAtUtc: doc.TakenAtUtc,
            leg: doc.Leg,
            comment: doc.Comment
        );
    }

    public async Task<TakenShot?> GetLatestAsync(Guid userId, CancellationToken ct = default)
    {
        var filter = Builders<TakenShotDocument>.Filter.Eq(x => x.UserId, userId);

        var doc = await _shots.Find(filter)
            .SortByDescending(x => x.TakenAtUtc)
            .FirstOrDefaultAsync(ct);

        if (doc is null)
            return null;

        return new TakenShot(
            id: doc.Id,
            userid: doc.UserId,
            takenAtUtc: doc.TakenAtUtc,
            leg: doc.Leg,
            comment: doc.Comment
        );
    }
}
