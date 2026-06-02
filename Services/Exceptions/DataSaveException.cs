namespace StudyTracker.Services.Exceptions;

public class DataSaveException : Exception
{
    public DataSaveException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
