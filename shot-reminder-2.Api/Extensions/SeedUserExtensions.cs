using shot_reminder_2.Application.Interfaces;
using shot_reminder_2.Domain.Entities;

namespace shot_reminder_2.Api.Extensions;

public static class SeedUserExtensions
{
    public static async Task SeedDefaultUserAsync(this WebApplication app, CancellationToken ct = default)
    {
        var config = app.Configuration.GetSection("SeedUser");

        var email = config["Email"]?.Trim().ToLowerInvariant();
        var password = config["Password"];
        var firstName = config["FirstName"];
        var lastName = config["LastName"];

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return;

        using var scope = app.Services.CreateScope();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("SeedUser");

        var exists = await userRepository.EmailExistsAsync(email, ct);
        if (exists)
        {
            logger.LogInformation("Seed user with email {Email} already exists. Skipping.", email);
            return;
        }

        var user = new User(
            id: Guid.NewGuid(),
            email: email,
            passwordHash: hasher.Hash(password),
            firstName: firstName,
            lastName: lastName);

        await userRepository.CreateAsync(user, ct);
        logger.LogInformation("Seed user with email {Email} was created.", email);
    }
}
