using shot_reminder_2.Application.Interfaces;
using shot_reminder_2.Application.Options;

namespace shot_reminder_2.Application.Use_Cases.Settings.Update_Shot_Settings;

public sealed class UpdateShotSettingsHandler
{
    private readonly IShotSettingsRepository _shotSettingsRepository;

    public UpdateShotSettingsHandler(IShotSettingsRepository shotSettingsRepository)
    {
        _shotSettingsRepository = shotSettingsRepository;
    }

    public async Task HandleAsync(UpdateShotSettingsCommand command, CancellationToken ct = default)
    {
        if (command.IntervalDays < 1)
            throw new ArgumentOutOfRangeException(nameof(command.IntervalDays), "IntervalDays must be at least 1.");

        if (command.LowStockThreshold < 0)
            throw new ArgumentOutOfRangeException(nameof(command.LowStockThreshold), "LowStockThreshold must be 0 or greater.");

        var settings = new ShotSettings
        {
            IntervalDays = command.IntervalDays,
            LowStockThreshold = command.LowStockThreshold
        };

        await _shotSettingsRepository.UpdateAsync(settings, ct);
    }
}
