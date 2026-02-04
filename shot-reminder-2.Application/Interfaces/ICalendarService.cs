
namespace shot_reminder_2.Application.Interfaces;

public interface ICalendarService
{
    Task UpsertNextShotEventAsync(Guid userId, DateTime nextDueAtUtc, string leg, CancellationToken ct = default);
}
