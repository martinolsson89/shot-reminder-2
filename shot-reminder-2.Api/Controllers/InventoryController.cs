using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using shot_reminder_2.Api.Extensions;
using shot_reminder_2.Application.Use_Cases.Inventory.AddStock;
using shot_reminder_2.Application.Use_Cases.Inventory.ConsumeOne;
using shot_reminder_2.Application.Use_Cases.Inventory.Delete;
using shot_reminder_2.Application.Use_Cases.Inventory.GetStock;
using shot_reminder_2.Application.Use_Cases.Inventory.Restock;
using shot_reminder_2.Application.Use_Cases.Inventory.Update;
using shot_reminder_2.Contracts.Inventory;

namespace shot_reminder_2.Api.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class InventoryController : ControllerBase
{

    private readonly AddStockHandler _addStockHandler;
    private readonly RestockHandler _restockHandler;
    private readonly ConsumeOneHandler _consumeOneHandler;
    private readonly DeleteInventoryHandler _deleteInventoryHandler;
    private readonly UpdateStockHandler _updateStockHandler;
    private readonly GetStockHandler _getStockHandler;


    public InventoryController(AddStockHandler addStockHandler, RestockHandler restockHandler, ConsumeOneHandler consumeOneHandler,
        DeleteInventoryHandler deleteInventoryHandler, UpdateStockHandler updateStockHandler, GetStockHandler getStockHandler)
    {
        _addStockHandler = addStockHandler;
        _restockHandler = restockHandler;
        _consumeOneHandler = consumeOneHandler;
        _deleteInventoryHandler = deleteInventoryHandler;
        _updateStockHandler = updateStockHandler;
        _getStockHandler = getStockHandler;
    }

    [HttpPost("add")]
    public async Task<IActionResult> AddStock(AddStockRequest request, CancellationToken ct)
    {
        var userId = User.GetUserId();

        await _addStockHandler.HandleAsync(new AddstockCommand(userId, request.shots), ct);
        
        return NoContent();

    }

    [HttpPost("restock")]
    public async Task<IActionResult> ReStock(AddStockRequest request, CancellationToken ct)
    {
        var userId = User.GetUserId();
        
        await _restockHandler.HandleAsync(new RestockCommand(userId, request.shots), ct);
        
        return NoContent();

    }

    [HttpPost("consume")]
    public async Task<IActionResult> ConsumeOne(CancellationToken ct)
    {
        var userId = User.GetUserId();

        await _consumeOneHandler.HandleAsync(userId, ct);
        
        return NoContent();
        
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteInventory(CancellationToken ct)
    {
        var userId = User.GetUserId();

        await _deleteInventoryHandler.HandleAsync(userId, ct);

        return NoContent();
    }

    [HttpPut("update")]
    public async Task<IActionResult> Update(AddStockRequest request, CancellationToken ct)
    {
        var userId = User.GetUserId();

        await _updateStockHandler.HandleAsync(new RestockCommand(userId, request.shots), ct);
        return NoContent();
    }

    [HttpGet]
    public async Task<IActionResult> GetStock(CancellationToken ct)
    {
        var userId = User.GetUserId();

        var res = await _getStockHandler.HandleAsync(userId, ct);

        if (res is null)
        {
            return NotFound();
        }
        return Ok(new GetInventoryResponse(UserId: res.UserId, ShotsLeft: res.ShotsLeft,UpdatedAtUtc: res.UpdatedAtUtc));
    }
}
