
using shot_reminder_2.Domain.Entities;

namespace shot_reminder_2.Application.Interfaces;

public interface IShotRepository
{
    Task<Guid> InsertAsync(TakenShot shot, CancellationToken ct = default);
    Task UpdateAsync(TakenShot shot, CancellationToken ct = default);
    Task DeleteAsync(Guid id, Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<TakenShot>> GetAsync(Guid userId, CancellationToken ct = default);
    Task<TakenShot?> GetByIdAsync(Guid id, Guid userId, CancellationToken ct = default);
    Task<TakenShot?> GetLatestAsync(Guid userId, CancellationToken ct = default);
}
