
using shot_reminder_2.Application.Interfaces;

namespace shot_reminder_2.Application.Use_Cases.Inventory.Delete;

public class DeleteInventoryHandler
{
    private IInventoryRepository _inventory;

    public DeleteInventoryHandler(IInventoryRepository inventory)
    {
        _inventory = inventory;
    }

    public async Task HandleAsync(Guid userId, CancellationToken ct = default)
    {
        await _inventory.DeleteAsync(userId, ct);
    }
}
