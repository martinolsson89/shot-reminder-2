namespace shot_reminder_2.Application.Use_Cases.Settings.Update_Shot_Settings;

public sealed record UpdateShotSettingsCommand(
    int IntervalDays,
    int LowStockThreshold);
