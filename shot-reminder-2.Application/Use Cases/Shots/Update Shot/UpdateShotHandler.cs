

using shot_reminder_2.Application.Interfaces;
using shot_reminder_2.Domain.Entities;

namespace shot_reminder_2.Application.Use_Cases.Shots.Update_Shot;

public class UpdateShotHandler
{
    private readonly IShotRepository _shotRepository;

    public UpdateShotHandler(IShotRepository shotRepository)
    {
        _shotRepository = shotRepository;
    }

    public async Task HandleAsync(UpdateShotCommand command, CancellationToken ct = default)
    {
        var shot = new TakenShot(
            id: command.Id,
            userid: command.UserId,
            takenAtUtc: command.TakenAtUtc,
            leg: command.Leg,
            comment: command.Comment);

        await _shotRepository.UpdateAsync(shot, ct);
    }
}
