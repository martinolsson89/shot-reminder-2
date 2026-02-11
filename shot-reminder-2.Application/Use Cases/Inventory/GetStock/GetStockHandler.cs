
using shot_reminder_2.Application.Interfaces;

namespace shot_reminder_2.Application.Use_Cases.Inventory.GetStock;

public class GetStockHandler
{
    private readonly IInventoryRepository _inventoryRepository;

    public GetStockHandler(IInventoryRepository inventoryRepository)
    {
        _inventoryRepository = inventoryRepository;
    }

    public async Task<GetStockResult?> HandleAsync(Guid userid, CancellationToken ct = default)
    {
        var inventory = await _inventoryRepository.GetAsync(userid, ct);

        if(inventory is null)
            return null;

        return new GetStockResult(UserId: inventory.UserId, ShotsLeft: inventory.ShotsLeft, UpdatedAtUtc: inventory.UpdatedAtUtc);

    }
}
