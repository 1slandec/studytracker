namespace StudyTracker.ViewModels;

public class CourseDetailsViewModel
{
    public CourseViewModel Course { get; set; } = new();

    public IReadOnlyList<StudyTaskListItemViewModel> Tasks { get; set; } = Array.Empty<StudyTaskListItemViewModel>();

    public bool CanManageCourse { get; set; }

    public bool CanExportTasks { get; set; }
}
