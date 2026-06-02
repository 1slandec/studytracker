namespace StudyTracker.ViewModels;

public class AssignCourseViewModel
{
    public int CourseId { get; set; }

    public string CourseName { get; set; } = string.Empty;

    public List<StudentAssignmentViewModel> Students { get; set; } = new();

    public string[] SelectedStudentIds { get; set; } = Array.Empty<string>();
}
