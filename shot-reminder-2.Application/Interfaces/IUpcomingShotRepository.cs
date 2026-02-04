
using shot_reminder_2.Domain.Entities;

namespace shot_reminder_2.Application.Interfaces;

public interface IUpcomingShotRepository
{
    Task<IReadOnlyList<UpcomingShot>> GetAsync(Guid userId, CancellationToken ct = default);
    Task AddUpcomingShot(UpcomingShot upcomingShot, CancellationToken ct = default);
    Task DeleteUpcomingShot(Guid userId, CancellationToken ct = default);
    Task UpsertUpcomingShotAsync(UpcomingShot upcomingShot, CancellationToken ct = default);
}
