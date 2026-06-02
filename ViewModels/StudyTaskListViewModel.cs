using StudyTracker.Models;

namespace StudyTracker.ViewModels;

public class StudyTaskListViewModel
{
    public int CourseId { get; set; }

    public string CourseName { get; set; } = string.Empty;

    public IReadOnlyList<StudyTaskListItemViewModel> Tasks { get; set; } = Array.Empty<StudyTaskListItemViewModel>();

    public TaskStatusType? StatusFilter { get; set; }

    public string SortOrder { get; set; } = string.Empty;

    public bool CanManageTasks { get; set; }

    public bool CanChangeStatus { get; set; }
}
