using shot_reminder_2.Application.Interfaces;
using shot_reminder_2.Application.Options;

namespace shot_reminder_2.Application.Use_Cases.Settings.Get_Shot_Settings;

public sealed class GetShotSettingsHandler
{
    private readonly IShotSettingsRepository _shotSettingsRepository;

    public GetShotSettingsHandler(IShotSettingsRepository shotSettingsRepository)
    {
        _shotSettingsRepository = shotSettingsRepository;
    }

    public Task<ShotSettings> HandleAsync(CancellationToken ct = default)
        => _shotSettingsRepository.GetAsync(ct);
}
