
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

    public RegisterShotHandler(IUserRepository userRepository, IShotRepository shotRepository, IInventoryRepository inventoryRepository, IEmailSender emailSender, ICalendarService calendarService)
    {
        _userRepository = userRepository;
        _shotRepository = shotRepository;
        _inventoryRepository = inventoryRepository;
        _emailSender = emailSender;
        _calendarService = calendarService;
    }

    public async Task<RegisterShotResult> HandleAsync(RegisterShotCommand command, CancellationToken ct = default)
    {
        int intervalDays = 14;

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

            var remaining = await _inventoryRepository.ConsumeOneAsync(command.userId, ct);

           if(remaining <= 2)
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
                }
            }

            // after Mongo writes succeed
            try
            {
                string leg = command.Leg == enLeg.Left ? enLeg.Right.ToString() : enLeg.Left.ToString();

                var nextDueAtUtc = command.TakenAtUtc.AddDays(intervalDays);
                await _calendarService.UpsertNextShotEventAsync(command.userId, nextDueAtUtc, leg, ct);
            }
            catch
            {
                // log, but do NOT throw
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
