namespace StudyTracker.Services;

public sealed record ReportFile(
    string FileName,
    string ContentType,
    byte[] Content);
