
namespace shot_reminder_2.Contracts.Shots;

public record UpdateShotRequest(DateTime TakenAtUtc, enLeg Leg, string? Comment);

