using shot_reminder_2.Domain.Entities;

namespace shot_reminder_2.Contracts.Shots;

public record ShotResponse(IReadOnlyList<ShotItemDto>? Shots);
