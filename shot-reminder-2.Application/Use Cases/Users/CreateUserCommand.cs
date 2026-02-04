
namespace shot_reminder_2.Application.Use_Cases.Users;
public record CreateUserCommand(string Email, string? FirstName, string? LastName);