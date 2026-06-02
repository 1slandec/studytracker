namespace StudyTracker.ViewModels;

public class CourseListViewModel
{
    public IReadOnlyList<CourseViewModel> Courses { get; set; } = Array.Empty<CourseViewModel>();

    public bool CanManageCourses { get; set; }
}
