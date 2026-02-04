using shot_reminder_2.Application.Interfaces;

namespace shot_reminder_2.Application.Use_Cases.Auth.Login;

public sealed class LoginHandler
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _hasher;
    private readonly ITokenService _tokens;

    public LoginHandler(IUserRepository users, IPasswordHasher hasher, ITokenService tokens)
    {
        _users = users;
        _hasher = hasher;
        _tokens = tokens;
    }

    public async Task<string> HandleAsync(string email, string password, CancellationToken ct = default)
    {
        var normalized = email.Trim().ToLowerInvariant();

        var user = await _users.GetByEmailAsync(normalized, ct);
        if (user is null)
            throw new InvalidOperationException("Invalid email or password.");

        if (!_hasher.Verify(password, user.PasswordHash))
            throw new InvalidOperationException("Invalid email or password.");

        return _tokens.CreateAccessToken(user.Id, user.Email);
    }
}
