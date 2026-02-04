
namespace shot_reminder_2.Application.Use_Cases.Shots.Update_Shot;

public record UpdateShotCommand(Guid Id, Guid UserId, DateTime TakenAtUtc, enLeg Leg, string? Comment);
