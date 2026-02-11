
using MongoDB.Driver;
using shot_reminder_2.Application.Commons;
using shot_reminder_2.Application.Interfaces;
using shot_reminder_2.Application.Use_Cases.Inventory.GetStock;
using shot_reminder_2.Domain.Entities;
using shot_reminder_2.Infrastructure.Presistence.Mongo.Collections;
using shot_reminder_2.Infrastructure.Presistence.Mongo.Context;
using shot_reminder_2.Infrastructure.Presistence.Mongo.Documents;
using System.Runtime.InteropServices.JavaScript;

namespace shot_reminder_2.Infrastructure.Presistence.Mongo.Repositories;

public class InventoryRepository : IInventoryRepository
{
    private readonly IMongoCollection<ShotsInventoryDocument> _inventory;

    public InventoryRepository(IMongoDbContext context)
    {
        _inventory = context.GetCollection<ShotsInventoryDocument>(CollectionNames.ShotsInventory);
    }

    public async Task AddStockAsync(ShotsInventory inventory, CancellationToken ct = default)
    {
        var doc = new ShotsInventoryDocument
        {
            UserId = inventory.UserId,
            ShotsLeft = inventory.ShotsLeft,
            UpdatedAtUtc = inventory.UpdatedAtUtc
        };

        try
        {
            await _inventory.InsertOneAsync(doc, cancellationToken: ct);
        }
        catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            // If you use UserId as _id or have a unique index, this happens when doc already exists
            throw new ConflictException("Inventory already exists. Use restock instead.");
        }

    }

    public async Task<int> ConsumeOneAsync(Guid userId, CancellationToken ct = default)
    {

        var exists = await _inventory.Find(x => x.UserId == userId).AnyAsync(ct);
        if (!exists) throw new NotFoundException("Inventory not found.");

        var filter = Builders<ShotsInventoryDocument>.Filter.And(
        Builders<ShotsInventoryDocument>.Filter.Eq(x => x.UserId, userId),
        Builders<ShotsInventoryDocument>.Filter.Gt(x => x.ShotsLeft, 0)
        );

        var update = Builders<ShotsInventoryDocument>.Update
            .Inc(x => x.ShotsLeft, -1)
            .Set(x => x.UpdatedAtUtc, DateTime.UtcNow);

        var options = new FindOneAndUpdateOptions<ShotsInventoryDocument>
        {
            ReturnDocument = ReturnDocument.After
        };

        var updated = await _inventory.FindOneAndUpdateAsync(filter, update, options, ct);

        if (updated is null)
            throw new InsufficientInventoryException("No shots left in inventory.");

        return updated.ShotsLeft;
    }

    public async Task DeleteAsync(Guid userId, CancellationToken ct = default)
    {
        var filter = Builders<ShotsInventoryDocument>.Filter.Eq(x => x.UserId, userId);
       

        var result = await _inventory.DeleteOneAsync(filter, cancellationToken: ct);

        if (result.DeletedCount == 0)
            throw new KeyNotFoundException();
    }

    public async Task<GetStockResult?> GetAsync(Guid userId, CancellationToken ct = default)
    {
        var filter = Builders<ShotsInventoryDocument>.Filter.Eq(x => x.UserId, userId);

        var doc = await _inventory.Find(filter).FirstOrDefaultAsync(ct);

        if (doc is null)
            return null;

        var inventoryResponse = new GetStockResult
            (
                UserId: doc.UserId,
                ShotsLeft: doc.ShotsLeft,
                UpdatedAtUtc: doc.UpdatedAtUtc
            );

        return inventoryResponse;
    }

    public async Task RestockAsync(Guid userId, int added, CancellationToken ct = default)
    {
        var filter = Builders<ShotsInventoryDocument>.Filter.Eq(x => x.UserId, userId);

        var update = Builders<ShotsInventoryDocument>.Update
        .Inc(x => x.ShotsLeft, added)
        .Set(x => x.UpdatedAtUtc, DateTime.UtcNow);

        await _inventory.UpdateOneAsync(
            filter,
            update,
            new UpdateOptions { IsUpsert = true },
            ct);
    }

    public async Task UpdateStock(Guid userId, int total, CancellationToken ct = default)
    {
        var filter = Builders<ShotsInventoryDocument>.Filter.Eq(x => x.UserId, userId);

        var update = Builders<ShotsInventoryDocument>.Update
            .Set(x => x.ShotsLeft, total)
            .Set(x => x.UpdatedAtUtc, DateTime.UtcNow);

        await _inventory.UpdateOneAsync(
            filter,
            update,
            new UpdateOptions { IsUpsert = true },
            ct);
    }
}
