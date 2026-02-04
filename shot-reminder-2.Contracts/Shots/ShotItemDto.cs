namespace shot_reminder_2.Contracts.Shots;

public record ShotItemDto(Guid Id, Guid UserId, DateTime TakenAtUtc, enLeg Leg, string? Comment);
