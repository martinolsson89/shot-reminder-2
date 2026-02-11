using shot_reminder_2.Application.Options;

namespace shot_reminder_2.Application.Interfaces;

public interface IShotSettingsRepository
{
    Task<ShotSettings> GetAsync(CancellationToken ct = default);
    Task UpdateAsync(ShotSettings settings, CancellationToken ct = default);
}
