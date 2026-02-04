
using shot_reminder_2.Application.Interfaces;
using shot_reminder_2.Application.Use_Cases.Inventory.AddStock;
using shot_reminder_2.Domain.Entities;

namespace shot_reminder_2.Application.Use_Cases.Inventory.ConsumeOne;

    public class ConsumeOneHandler
    {
    private IInventoryRepository _inventory;

    public ConsumeOneHandler(IInventoryRepository inventory)
    {
        _inventory = inventory;
    }

    public async Task HandleAsync(Guid userId, CancellationToken ct = default)
    {
        await _inventory.ConsumeOneAsync(userId, ct);
    }
}

