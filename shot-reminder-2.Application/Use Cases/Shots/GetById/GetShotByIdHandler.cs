
using shot_reminder_2.Application.Dto;
using shot_reminder_2.Application.Interfaces;

namespace shot_reminder_2.Application.Use_Cases.Shots.GetById;

public class GetShotByIdHandler
{
    private readonly IShotRepository _shotRepository;

    public GetShotByIdHandler(IShotRepository shotRepository)
    {
        _shotRepository = shotRepository;
    }

    public async Task<TakenShotDto?> HandleAsync(Guid id, Guid userId, CancellationToken ct = default)
    {
        var shot = await _shotRepository.GetByIdAsync(id, userId, ct);

        if (shot == null)
            return null;

        var response = new TakenShotDto(Id: shot.Id, UserId: shot.UserId, TakenAtUtc:shot.TakenAtUtc, Leg:shot.Leg, Comment:shot.Comment);

        return response;
    }
}
