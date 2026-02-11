
using Microsoft.Extensions.Logging;
using shot_reminder_2.Application.Commons;
using shot_reminder_2.Application.Interfaces;
using shot_reminder_2.Domain.Entities;

namespace shot_reminder_2.Application.Use_Cases.Shots.Register_Shot;

public class RegisterShotHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IShotRepository _shotRepository;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IEmailSender _emailSender;
    private readonly ICalendarService _calendarService;
    private readonly IShotSettingsRepository _shotSettingsRepository;
    private readonly ILogger<RegisterShotHandler> _logger;

    public RegisterShotHandler(IUserRepository userRepository, IShotRepository shotRepository, IInventoryRepository inventoryRepository, 
        IEmailSender emailSender, ICalendarService calendarService, IShotSettingsRepository shotSettingsRepository, ILogger<RegisterShotHandler> logger)
    {
        _userRepository = userRepository;
        _shotRepository = shotRepository;
        _inventoryRepository = inventoryRepository;
        _emailSender = emailSender;
        _calendarService = calendarService;
        _shotSettingsRepository = shotSettingsRepository;
        _logger = logger;
    }

    public async Task<RegisterShotResult> HandleAsync(RegisterShotCommand command, CancellationToken ct = default)
    {
        var settings = await _shotSettingsRepository.GetAsync(ct);
        var intervalDays = settings.IntervalDays;
        var lowStockThreshold = settings.LowStockThreshold;

        var user = await _userRepository.GetByIdAsync(command.userId, ct);
        if (user is null)
            throw new NotFoundException($"User '{command.userId}' was not found.");

        var shot = new TakenShot(
            id:Guid.NewGuid(),
            userid: command.userId,
            takenAtUtc: command.TakenAtUtc,
            leg: command.Leg,
            comment: command.Comment);


        Guid shotId = Guid.Empty;

        try
        {
            shotId = await _shotRepository.InsertAsync(shot, ct);
            _logger.LogInformation("Registering shot for user {UserId}", command.userId);

            var remaining = await _inventoryRepository.ConsumeOneAsync(command.userId, ct);
            _logger.LogInformation("Inventory remaining after consume: {Remaining}", remaining);

            if (remaining <= lowStockThreshold)
            {
                try
                {
                    await _emailSender.SendEmailAsync(
                        to: user.Email,
                        subject: "Få sprutor kvar i kylen",
                        body: $"Det är dags att beställa nya sprutor, du har bara {remaining} kvar i kylen.",
                        ct: ct);
                }
                catch
                {
                    // swallow (or log) — don't break shot registration due to email
                    _logger.LogWarning("Failed to send email to user {UserId} regarding low stock", command.userId);
                }
            }

            // after Mongo writes succeed
            try
            {
                var hasNewerShot = await _shotRepository.ExistsWithTakenAtUtcAfterAsync(command.userId, command.TakenAtUtc, ct);
                if (!hasNewerShot)
                {
                    string leg = command.Leg == enLeg.Left ? enLeg.Right.ToString() : enLeg.Left.ToString();
                    var nextDueAtUtc = command.TakenAtUtc.AddDays(intervalDays);

                    await _calendarService.UpsertNextShotEventAsync(command.userId, nextDueAtUtc, leg, ct);
                }
                else
                {
                    _logger.LogInformation(
                        "Skipping calendar update for user {UserId} because a newer shot already exists than {TakenAtUtc}",
                        command.userId,
                        command.TakenAtUtc);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Calendar update failed for user {UserId}", command.userId);
            }



            return new RegisterShotResult(shotId);
        }
        catch
        {
            if (shotId != Guid.Empty)
            {
                // best-effort rollback
                try 
                { 
                    await _shotRepository.DeleteAsync(shotId, command.userId, ct);

                } catch { /* ignore */ }
            }
            throw;
        }
    }
}
