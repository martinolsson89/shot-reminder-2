
using shot_reminder_2.Domain.Entities;

namespace shot_reminder_2.Application.Interfaces;

    public interface IPasswordHasher
    {
        string Hash(string password);
        bool Verify(string password, string passwordHash);
    }

