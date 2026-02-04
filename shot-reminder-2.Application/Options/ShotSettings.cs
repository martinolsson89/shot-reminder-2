
namespace shot_reminder_2.Application.Options;

public sealed class ShotSettings
{
    public int IntervalDays { get; init; } = 14;
    public int LowStockThreshold { get; init; } = 2;
}
