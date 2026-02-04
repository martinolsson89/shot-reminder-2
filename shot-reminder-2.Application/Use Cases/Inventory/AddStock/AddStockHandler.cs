
using shot_reminder_2.Application.Interfaces;
using shot_reminder_2.Domain.Entities;

namespace shot_reminder_2.Application.Use_Cases.Inventory.AddStock;

public class AddStockHandler
{
    private IInventoryRepository _inventory;

    public AddStockHandler(IInventoryRepository inventory)
    {
        _inventory = inventory;
    }

    public async Task HandleAsync(AddstockCommand command, CancellationToken ct = default)
    {
        if (command.shots <= 0)
            throw new ArgumentOutOfRangeException(nameof(command.shots), "Shots must be greater than 0.");

        var shotsInventory = new ShotsInventory(userId: command.userId, initialShots: command.shots);

        await _inventory.AddStockAsync(shotsInventory, ct);
    }
}
