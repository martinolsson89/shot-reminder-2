
namespace shot_reminder_2.Application.Interfaces;

public interface IMongoIndexInitializer
{
    Task EnsureIndexesAsync(CancellationToken ct = default);
}
