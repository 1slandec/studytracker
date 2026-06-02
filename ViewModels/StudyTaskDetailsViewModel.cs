using StudyTracker.Models;

namespace StudyTracker.ViewModels;

public class StudyTaskDetailsViewModel
{
    public int Id { get; set; }

    public int CourseId { get; set; }

    public string CourseName { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateTime Deadline { get; set; }

    public TaskStatusType Status { get; set; }

    public string StatusName { get; set; } = string.Empty;

    public bool IsOverdue { get; set; }

    public bool CanManageTask { get; set; }

    public bool CanChangeStatus { get; set; }
}
