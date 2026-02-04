
using shot_reminder_2.Application.Commons;
using shot_reminder_2.Application.Interfaces;
using shot_reminder_2.Domain.Entities;

namespace shot_reminder_2.Application.Use_Cases.Users;

public class CreateUserHandler
{
    private IUserRepository _userRepository;

    public CreateUserHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    //public async Task<CreateUserResult> HandleAsync(CreateUserCommand command, CancellationToken ct = default)
    //{
    //    if (string.IsNullOrWhiteSpace(command.Email))
    //        throw new ArgumentException("Email is required.", nameof(command.Email));

    //    var email = command.Email.Trim().ToLowerInvariant();

    //    if (await _userRepository.EmailExistsAsync(email, ct))
    //        throw new ConflictException($"User with email '{email}' already exists.");

    //    //var user = new User(email: command.Email, firstName: command.FirstName, lastName: command.LastName);

    //    var id = await _userRepository.InsertAsync(user, ct);
    //    return new CreateUserResult(id);
    //}
}
