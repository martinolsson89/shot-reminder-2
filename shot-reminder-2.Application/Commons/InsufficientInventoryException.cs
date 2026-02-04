
namespace shot_reminder_2.Application.Commons;

public sealed class InsufficientInventoryException : Exception
{
    public InsufficientInventoryException(string message) : base(message) { }
}
