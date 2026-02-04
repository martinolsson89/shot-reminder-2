using shot_reminder_2.Application.Interfaces;
using shot_reminder_2.Domain.Entities;

namespace shot_reminder_2.Application.Use_Cases.Auth.Register;

public sealed class RegisterUserHandler
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _hasher;

    public RegisterUserHandler(IUserRepository users, IPasswordHasher hasher)
    {
        _users = users;
        _hasher = hasher;
    }

    public async Task<Guid> HandleAsync(string email, string password, string? firstName, string? lastName, CancellationToken ct = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();

        var existing = await _users.GetByEmailAsync(normalizedEmail, ct);
        if (existing is not null)
            throw new InvalidOperationException("Email is already registered.");

        // You can add stronger validation later
        if (password.Length < 8)
            throw new ArgumentException("Password must be at least 8 characters.");

        var passwordHash = _hasher.Hash(password);

        var user = new User(
            id: Guid.NewGuid(),
            email: normalizedEmail,
            passwordHash: passwordHash,
            firstName: firstName,
            lastName: lastName
        );

        
        await _users.CreateAsync(user, ct);

        return user.Id;
    }
}
