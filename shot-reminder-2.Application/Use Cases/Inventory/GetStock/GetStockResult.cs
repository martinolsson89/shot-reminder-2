
namespace shot_reminder_2.Application.Use_Cases.Inventory.GetStock;
    public record GetStockResult(Guid UserId, int ShotsLeft, DateTime UpdatedAtUtc);

