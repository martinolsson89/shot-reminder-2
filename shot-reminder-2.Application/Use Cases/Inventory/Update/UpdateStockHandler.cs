
using shot_reminder_2.Application.Interfaces;
using shot_reminder_2.Application.Use_Cases.Inventory.Restock;

namespace shot_reminder_2.Application.Use_Cases.Inventory.Update;

public class UpdateStockHandler
{
    private IInventoryRepository _inventory;

    public UpdateStockHandler(IInventoryRepository inventory)
    {
        _inventory = inventory;
    }

    public async Task HandleAsync(RestockCommand command, CancellationToken ct = default)
    {
        if (command.shots < 0)
            throw new ArgumentOutOfRangeException(nameof(command.shots), "Shots can be a negative value");

        await _inventory.UpdateStock(command.userId, command.shots, ct);
    }
}
