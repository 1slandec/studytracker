using StudyTracker.Models;

namespace StudyTracker.ViewModels;

public class ChangeTaskStatusViewModel
{
    public int TaskId { get; set; }

    public TaskStatusType Status { get; set; }

    public string? ReturnUrl { get; set; }
}
