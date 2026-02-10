using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using shot_reminder_2.Application.Use_Cases.Settings.Get_Shot_Settings;
using shot_reminder_2.Application.Use_Cases.Settings.Update_Shot_Settings;
using shot_reminder_2.Contracts.Settings;

namespace shot_reminder_2.Api.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public sealed class SettingsController : ControllerBase
{
    private readonly GetShotSettingsHandler _getShotSettingsHandler;
    private readonly UpdateShotSettingsHandler _updateShotSettingsHandler;

    public SettingsController(
        GetShotSettingsHandler getShotSettingsHandler,
        UpdateShotSettingsHandler updateShotSettingsHandler)
    {
        _getShotSettingsHandler = getShotSettingsHandler;
        _updateShotSettingsHandler = updateShotSettingsHandler;
    }

    [HttpGet("shots")]
    public async Task<IActionResult> GetShotSettings(CancellationToken ct)
    {
        var settings = await _getShotSettingsHandler.HandleAsync(ct);

        return Ok(new GetShotSettingsResponse(
            IntervalDays: settings.IntervalDays,
            LowStockThreshold: settings.LowStockThreshold));
    }

    [HttpPut("shots")]
    public async Task<IActionResult> UpdateShotSettings(UpdateShotSettingsRequest request, CancellationToken ct)
    {
        await _updateShotSettingsHandler.HandleAsync(new UpdateShotSettingsCommand(
            IntervalDays: request.IntervalDays,
            LowStockThreshold: request.LowStockThreshold), ct);

        return NoContent();
    }
}
