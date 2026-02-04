namespace shot_reminder_2.Application.Commons;

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}
