
namespace shot_reminder_2.Application.Use_Cases.Shots.Register_Shot;

public record RegisterShotCommand(Guid userId, DateTime TakenAtUtc, enLeg Leg, string? Comment);
