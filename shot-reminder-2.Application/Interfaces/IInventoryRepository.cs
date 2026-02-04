
using shot_reminder_2.Domain.Entities;

namespace shot_reminder_2.Application.Interfaces;

public interface IInventoryRepository
{
    Task AddStockAsync(ShotsInventory inventory, CancellationToken ct = default);
    Task RestockAsync(Guid userId, int added, CancellationToken ct = default);
    Task<int> ConsumeOneAsync(Guid userId, CancellationToken ct = default);
    Task UpdateStock(Guid userId, int total, CancellationToken ct = default);
    Task DeleteAsync(Guid userId, CancellationToken ct = default);
    Task<int> GetAsync(Guid userId, CancellationToken ct = default);
}
