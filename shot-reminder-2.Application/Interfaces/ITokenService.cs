
using shot_reminder_2.Domain.Entities;

namespace shot_reminder_2.Application.Interfaces;

public interface ITokenService
{
    string CreateAccessToken(Guid userId, string email);
}