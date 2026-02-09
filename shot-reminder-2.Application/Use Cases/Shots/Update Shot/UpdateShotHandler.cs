

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using shot_reminder_2.Application.Interfaces;
using shot_reminder_2.Application.Options;
using shot_reminder_2.Application.Use_Cases.Shots.Register_Shot;
using shot_reminder_2.Domain.Entities;

namespace shot_reminder_2.Application.Use_Cases.Shots.Update_Shot;

public class UpdateShotHandler
{
    private readonly IShotRepository _shotRepository;
    private readonly ICalendarService _calendarService;
    private readonly ShotSettings _settings;
    private readonly ILogger<RegisterShotHandler> _logger;

    public UpdateShotHandler(IShotRepository shotRepository, ICalendarService calendarService, IOptions<ShotSettings> shotSettings, ILogger<RegisterShotHandler> logger)
    {
        _shotRepository = shotRepository;
        _calendarService = calendarService;
        _settings = shotSettings.Value;
        _logger = logger;
    }

    public async Task HandleAsync(UpdateShotCommand command, CancellationToken ct = default)
    {
        var intervalDays = _settings.IntervalDays;

        var shot = new TakenShot(
            id: command.Id,
            userid: command.UserId,
            takenAtUtc: command.TakenAtUtc,
            leg: command.Leg,
            comment: command.Comment);

        await _shotRepository.UpdateAsync(shot, ct);

        try
        {
            var hasNewerShot = await _shotRepository.ExistsWithTakenAtUtcAfterAsync(command.UserId, command.TakenAtUtc, ct);
            if (!hasNewerShot)
            {
                string leg = command.Leg == enLeg.Left ? enLeg.Right.ToString() : enLeg.Left.ToString();
                var nextDueAtUtc = command.TakenAtUtc.AddDays(intervalDays);

                await _calendarService.UpsertNextShotEventAsync(command.UserId, nextDueAtUtc, leg, ct);
            }
            else
            {
                _logger.LogInformation(
                    "Skipping calendar update for user {UserId} because a newer shot already exists than {TakenAtUtc}",
                    command.UserId,
                    command.TakenAtUtc);
            }
        }
        catch
        {
            _logger.LogWarning("Calendar update failed for user {UserId}", command.UserId);
        }
    }
}
