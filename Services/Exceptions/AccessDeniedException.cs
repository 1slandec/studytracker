namespace StudyTracker.Services.Exceptions;

public class AccessDeniedException : Exception
{
    public AccessDeniedException(string message)
        : base(message)
    {
    }
}
