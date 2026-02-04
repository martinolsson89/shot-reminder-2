using shot_reminder_2.Domain.Entities;

namespace shot_reminder_2.Application.Interfaces;

public interface IUserRepository
{
    Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);
    Task<bool> ExistingAsync(Guid userId, CancellationToken ct = default);
    Task CreateAsync(User user, CancellationToken ct = default);
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
}
