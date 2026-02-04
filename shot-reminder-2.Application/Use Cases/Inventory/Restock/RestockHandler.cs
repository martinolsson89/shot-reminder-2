
using shot_reminder_2.Application.Interfaces;
using shot_reminder_2.Application.Use_Cases.Inventory.AddStock;
using shot_reminder_2.Domain.Entities;

namespace shot_reminder_2.Application.Use_Cases.Inventory.Restock;

public class RestockHandler
{
    private IInventoryRepository _inventory;

    public RestockHandler(IInventoryRepository inventory)
    {
        _inventory = inventory;
    }

    public async Task HandleAsync(RestockCommand command, CancellationToken ct = default)
    {
        if (command.shots <= 0)
            throw new ArgumentOutOfRangeException(nameof(command.shots), "Shots must be greater than 0.");

        await _inventory.RestockAsync(command.userId, command.shots, ct);
    }
}
