
namespace shot_reminder_2.Domain.Entities;

public sealed class TakenShot
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime TakenAtUtc { get; private set; }
    public enLeg Leg { get; private set; }
    public string? Comment { get; private set; }
    private TakenShot() { }
    public TakenShot(Guid id, Guid userid, DateTime takenAtUtc, enLeg leg, string? comment)
    {
        Id = id;
        UserId = userid;
        TakenAtUtc = takenAtUtc;
        Leg = leg;
        Comment = comment;
    }

}
