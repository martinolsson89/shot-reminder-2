
using shot_reminder_2.Application.Dto;
using shot_reminder_2.Application.Interfaces;

namespace shot_reminder_2.Application.Use_Cases.Shots.GetAll;

public class GetShotsHandler
{
    private readonly IShotRepository _shotRepository;

    public GetShotsHandler(IShotRepository shotRepository)
    {
        _shotRepository = shotRepository;
    }

    public async Task<GetShotsResult> HandleAsync(Guid userId, CancellationToken ct = default)
    {

        var shots = await _shotRepository.GetAsync(userId, ct);

        var response = shots
        .Select(s => new TakenShotDto(s.Id, s.UserId, s.TakenAtUtc, s.Leg, s.Comment))
        .ToList();


        return new GetShotsResult(response);
    }
}
