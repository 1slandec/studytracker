namespace StudyTracker.ViewModels;

public class CourseViewModel
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string ProfessorName { get; set; } = string.Empty;

    public int TaskCount { get; set; }

    public bool IsAssignedToCurrentStudent { get; set; }
}
