
namespace shot_reminder_2.Domain.Entities;

public sealed class User
{
    public Guid Id { get; private set; }
    public string Email { get; private set; } = default!;
    public string PasswordHash { get; private set; }
    public string? FirstName { get; private set; } = default!;
    public string? LastName { get; private set;} = default!;


    public int ShotIntervalDays { get; private set; } = 14;
    public DateTime CreatedAtUtc { get; private set; }

    private User() { }

    public User(Guid id, string email, string passwordHash, string? firstName, string? lastName)
    {
        Id = id;
        Email = email.Trim().ToLowerInvariant();
        PasswordHash = passwordHash;
        FirstName = string.IsNullOrWhiteSpace(firstName) ? null : firstName.Trim();
        LastName = string.IsNullOrWhiteSpace(lastName) ? null : lastName.Trim();

        CreatedAtUtc = DateTime.UtcNow;
    }

    public void ChangeShotInterval(int days)
    {
        if(days < 1)
            throw new ArgumentException("Shot interval must be at least 1 day.");
        
        ShotIntervalDays = days;
    }
}
