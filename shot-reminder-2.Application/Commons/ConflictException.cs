namespace shot_reminder_2.Application.Commons;

    public sealed class ConflictException : Exception
    {
        public ConflictException(string message) : base(message) { }
    }

