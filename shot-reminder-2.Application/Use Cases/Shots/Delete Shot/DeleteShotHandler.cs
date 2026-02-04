
using shot_reminder_2.Application.Interfaces;

namespace shot_reminder_2.Application.Use_Cases.Shots.Delete_Shot;

public class DeleteShotHandler
{
    private readonly IShotRepository _shotRepository;

    public DeleteShotHandler(IShotRepository shotRepository)
    {
        _shotRepository = shotRepository;
    }

    public async Task HandleAsync(DeleteShotCommand command, CancellationToken ct = default)
    {
        await _shotRepository.DeleteAsync(command.id, command.userId, ct);
    }
}

