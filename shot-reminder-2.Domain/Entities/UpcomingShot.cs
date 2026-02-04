
namespace shot_reminder_2.Domain.Entities;

public sealed class UpcomingShot
{
    public Guid UserId { get; private set; }
    public DateTime NextDueAtUtc { get; private set; }
    public int IntervalDays { get; private set; }

    private UpcomingShot() { }

    public UpcomingShot(Guid userid, int intervalDays, DateTime nextDueAtUtc)
    {
        UserId = userid;
        IntervalDays = intervalDays;
        NextDueAtUtc = nextDueAtUtc;
    }

    //NextDueAtUtc = DateTime.UtcNow.AddDays(intervalDays);
}
