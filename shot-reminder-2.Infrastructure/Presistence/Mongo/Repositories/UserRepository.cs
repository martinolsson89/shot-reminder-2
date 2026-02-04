
using MongoDB.Driver;
using shot_reminder_2.Application.Interfaces;
using shot_reminder_2.Domain.Entities;
using shot_reminder_2.Infrastructure.Presistence.Mongo.Collections;
using shot_reminder_2.Infrastructure.Presistence.Mongo.Context;
using shot_reminder_2.Infrastructure.Presistence.Mongo.Documents;

namespace shot_reminder_2.Infrastructure.Presistence.Mongo.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly IMongoCollection<UserDocument> _users;

    public UserRepository(IMongoDbContext context)
    {
        _users = context.GetCollection<UserDocument>(CollectionNames.Users);
    }
    public async Task<bool> EmailExistsAsync(string email, CancellationToken ct = default)
    {
        var filter = Builders<UserDocument>.Filter.Eq(x => x.Email, email);
        return await _users.Find(filter).AnyAsync(ct);
    }

    public async Task<bool> ExistingAsync(Guid userId, CancellationToken ct = default)
    {
        var filter = Builders<UserDocument>.Filter.Eq(x => x.Id, userId);
        return await _users.Find(filter).AnyAsync(ct);
    }

    public async Task CreateAsync(User user, CancellationToken ct = default)
    {
        try
        {
            await _users.InsertOneAsync(MapToDocument(user), cancellationToken: ct);
        }
        catch (MongoWriteException ex)
            when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            throw new InvalidOperationException("Email is already registered.");
        }
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var doc = await _users.Find(x => x.Id == id).FirstOrDefaultAsync(ct);
        return doc is null ? null : MapToDomain(doc);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        var normalized = email.Trim().ToLowerInvariant();
        var doc = await _users.Find(x => x.Email == normalized).FirstOrDefaultAsync(ct);
        return doc is null ? null : MapToDomain(doc);
    }

    private static User MapToDomain(UserDocument doc)
        => new(
            id: doc.Id,
            email: doc.Email,
            passwordHash: doc.PasswordHash,
            firstName: doc.FirstName,
            lastName: doc.LastName
        );

    private static UserDocument MapToDocument(User user)
        => new()
        {
            Id = user.Id,
            Email = user.Email,
            PasswordHash = user.PasswordHash,
            FirstName = user.FirstName,
            LastName = user.LastName,
            CreatedAtUtc = user.CreatedAtUtc 
        };
}
