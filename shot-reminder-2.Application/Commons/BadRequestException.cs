
namespace shot_reminder_2.Application.Commons;

public sealed class BadRequestException : Exception
{
    public BadRequestException(string message) : base(message) { }
}
