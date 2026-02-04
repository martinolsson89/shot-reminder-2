
namespace shot_reminder_2.Application.Dto;

public record TakenShotDto(Guid Id, Guid UserId, DateTime TakenAtUtc, enLeg Leg, string? Comment);
