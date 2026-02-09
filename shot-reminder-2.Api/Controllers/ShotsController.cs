using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using shot_reminder_2.Api.Extensions;
using shot_reminder_2.Application.Use_Cases.Shots.Delete_Shot;
using shot_reminder_2.Application.Use_Cases.Shots.Get_Latest;
using shot_reminder_2.Application.Use_Cases.Shots.GetAll;
using shot_reminder_2.Application.Use_Cases.Shots.GetById;
using shot_reminder_2.Application.Use_Cases.Shots.Register_Shot;
using shot_reminder_2.Application.Use_Cases.Shots.Update_Shot;
using shot_reminder_2.Contracts.Common;
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
    private readonly GetLatestHandler _getLatestHandler;

    public ShotsController(RegisterShotHandler shotHandler, GetShotsHandler getShotsHandler, UpdateShotHandler updateShotHandler, DeleteShotHandler deleteShotHandler,
        GetShotByIdHandler getShotByIdHandler, GetLatestHandler getLatestHandler)
    {
        _shotHandler = shotHandler;
        _updateShotHandler = updateShotHandler;
        _getShotsHandler = getShotsHandler;
        _deleteShotHandler = deleteShotHandler;
        _getShotByIdHandler = getShotByIdHandler;
        _getLatestHandler = getLatestHandler;
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

    [HttpPost("addshot")]
    public async Task<IActionResult> AddShot([FromBody] UpdateShotRequest request, CancellationToken ct)
    {
        var userId = User.GetUserId();

        var registerShotResult = await _shotHandler.HandleAsync(
            new RegisterShotCommand(
                userId: userId,
                TakenAtUtc: request.TakenAtUtc,
                Leg: request.Leg,
                Comment: request.Comment),
            ct);

        var response = new RegisterShotResponse(registerShotResult.id);
        return Created(nameof(AddShot), response);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateShot(Guid id, [FromBody] UpdateShotRequest request, CancellationToken ct)
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
    public async Task<IActionResult> DeleteShot([FromRoute] Guid id, CancellationToken ct)
    {
        var userId = User.GetUserId();

        await _deleteShotHandler.HandleAsync(
            new DeleteShotCommand(id, userId),
            ct);

        return NoContent();
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<ShotItemDto>>> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        if (page < 1)
            return BadRequest("`page` must be greater than or equal to 1.");

        if (pageSize < 1 || pageSize > 100)
            return BadRequest("`pageSize` must be between 1 and 100.");

        var userId = User.GetUserId();

        var result = await _getShotsHandler.HandleAsync(userId, ct);

        var pagedResponse = result.Shots.ToPagedResponse(
            page,
            pageSize,
            s => new ShotItemDto(s.Id, s.UserId, s.TakenAtUtc, s.Leg, s.Comment));

        return Ok(pagedResponse);
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

    [HttpGet("latest")]
    public async Task<IActionResult> GetLatest(CancellationToken ct)
    {
        var userId = User.GetUserId();

        var result = await _getLatestHandler.HandleAsync(userId, ct);

        if(result is null)
        {
            return NotFound();
        }

        var response = new ShotItemDto(result.Id, result.UserId, result.TakenAtUtc, result.Leg, result.Comment);
        return Ok(response);
    }
}
