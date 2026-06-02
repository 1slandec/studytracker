namespace StudyTracker.ViewModels;

public class StudentAssignmentViewModel
{
    public string StudentId { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public bool IsAssigned { get; set; }
}
