using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using shot_reminder_2.Api.Extensions;
using shot_reminder_2.Application.Use_Cases.Shots.Delete_Shot;
using shot_reminder_2.Application.Use_Cases.Shots.GetAll;
using shot_reminder_2.Application.Use_Cases.Shots.GetById;
using shot_reminder_2.Application.Use_Cases.Shots.Register_Shot;
using shot_reminder_2.Application.Use_Cases.Shots.Update_Shot;
using shot_reminder_2.Contracts.Shots;

namespace shot_reminder_2.Api.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class ShotsController : ControllerBase
{

    private readonly RegisterShotHandler _shotHandler;
    private readonly UpdateShotHandler _updateShotHandler;
    private readonly GetShotsHandler _getShotsHandler;
    private readonly DeleteShotHandler _deleteShotHandler;
    private readonly GetShotByIdHandler _getShotByIdHandler;

    public ShotsController(RegisterShotHandler shotHandler, GetShotsHandler getShotsHandler, UpdateShotHandler updateShotHandler, DeleteShotHandler deleteShotHandler,
        GetShotByIdHandler getShotByIdHandler)
    {
        _shotHandler = shotHandler;
        _updateShotHandler = updateShotHandler;
        _getShotsHandler = getShotsHandler;
        _deleteShotHandler = deleteShotHandler;
        _getShotByIdHandler = getShotByIdHandler;
    }

    [HttpPost]
    public async Task<IActionResult> RegisterShot(RegisterShotRequest request, CancellationToken ct)
    {

        var userId = User.GetUserId();

        var registerShotResult = await _shotHandler.HandleAsync(new RegisterShotCommand
            (
            userId: userId,
            TakenAtUtc: DateTime.UtcNow,
            Leg: request.Leg,
            Comment: request.Comment
            ),
            ct);

        var response = new RegisterShotResponse(registerShotResult.id);

        return Created(nameof(RegisterShot), response);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateShot(Guid id, UpdateShotRequest request, CancellationToken ct)
    {
        var userId = User.GetUserId();
        
        await _updateShotHandler.HandleAsync(new UpdateShotCommand
            (
                Id: id,
                UserId: userId,
                TakenAtUtc: request.TakenAtUtc,
                Leg: request.Leg,
                Comment: request.Comment
            ),
            ct);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteShot(Guid id, CancellationToken ct)
    {
        var userId = User.GetUserId();

        await _deleteShotHandler.HandleAsync(
            new DeleteShotCommand(id, userId),
            ct);

        return NoContent();
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var userId = User.GetUserId();

        var result = await _getShotsHandler.HandleAsync(userId);
        
        if (result.Shots is null || result.Shots.Count == 0)
            return Ok(new ShotResponse(Array.Empty<ShotItemDto>()));

        var response = result.Shots
        .Select(s => new ShotItemDto(s.Id, s.UserId, s.TakenAtUtc, s.Leg, s.Comment))
        .ToList();

        return Ok(response);
    }
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetbyId(Guid id, CancellationToken ct)
    {
        var userId = User.GetUserId();

        var result = await _getShotByIdHandler.HandleAsync(id, userId, ct);

        if(result is null)
        {
            return NotFound();
        }

        var response = new ShotItemDto(result.Id, result.UserId, result.TakenAtUtc, result.Leg, result.Comment);
        return Ok(response);
    }
}
