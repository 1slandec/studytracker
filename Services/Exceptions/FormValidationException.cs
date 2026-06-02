namespace StudyTracker.Services.Exceptions;

public class FormValidationException : Exception
{
    public FormValidationException(string message)
        : base(message)
    {
    }
}
