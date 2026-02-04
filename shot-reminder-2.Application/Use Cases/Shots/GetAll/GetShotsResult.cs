
using shot_reminder_2.Application.Dto;

namespace shot_reminder_2.Application.Use_Cases.Shots.GetAll;

public record GetShotsResult(IReadOnlyList<TakenShotDto>? Shots);
