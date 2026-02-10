namespace shot_reminder_2.Contracts.Settings;

public sealed record GetShotSettingsResponse(
    int IntervalDays,
    int LowStockThreshold);
