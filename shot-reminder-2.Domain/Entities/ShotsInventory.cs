
namespace shot_reminder_2.Domain.Entities;

public sealed class ShotsInventory
{
    public Guid UserId { get; private set; }
    public int ShotsLeft { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    private ShotsInventory() { }

    public ShotsInventory(Guid userId, int initialShots)
    {
        UserId = userId;
        ShotsLeft = initialShots;

        UpdatedAtUtc = DateTime.UtcNow;
    }
}
