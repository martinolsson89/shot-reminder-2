
namespace shot_reminder_2.Contracts.Auth;

public record RegisterRequest(string Email, string Password, string? FirstName, string? LastName);
