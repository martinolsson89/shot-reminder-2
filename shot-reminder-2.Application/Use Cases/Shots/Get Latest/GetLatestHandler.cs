
using shot_reminder_2.Application.Dto;
using shot_reminder_2.Application.Interfaces;

namespace shot_reminder_2.Application.Use_Cases.Shots.Get_Latest;

public class GetLatestHandler
{
    IShotRepository _shotRepository;

    public GetLatestHandler(IShotRepository shotRepository)
    {
        _shotRepository = shotRepository;
    }

    public async Task<TakenShotDto?> HandleAsync(Guid userId, CancellationToken ct = default)
    {
        var shot = await _shotRepository.GetLatestAsync(userId, ct);

        if (shot is null)
            return null;

        return new TakenShotDto(
            shot.Id,
            shot.UserId,
            shot.TakenAtUtc,
            shot.Leg,
            shot.Comment
        );
    }
}
