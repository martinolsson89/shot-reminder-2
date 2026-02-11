namespace shot_reminder_2.Contracts.Settings;

public sealed record UpdateShotSettingsRequest(
    int IntervalDays,
    int LowStockThreshold);
