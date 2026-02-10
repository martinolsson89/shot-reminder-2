
namespace shot_reminder_2.Contracts.Inventory;

public record GetInventoryResponse(Guid UserId, int ShotsLeft, DateTime UpdatedAtUtc);
