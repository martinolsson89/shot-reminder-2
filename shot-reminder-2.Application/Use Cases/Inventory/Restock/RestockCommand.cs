

namespace shot_reminder_2.Application.Use_Cases.Inventory.Restock;

public record RestockCommand(Guid userId, int shots);
