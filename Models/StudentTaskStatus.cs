using System.ComponentModel.DataAnnotations;

namespace StudyTracker.Models;

public class StudentTaskStatus
{
    public int Id { get; set; }

    [Required]
    public string StudentId { get; set; } = string.Empty;

    public User? Student { get; set; }

    [Required]
    public int StudyTaskId { get; set; }

    public StudyTask? StudyTask { get; set; }

    [Required]
    public TaskStatusType Status { get; set; } = TaskStatusType.NotStarted;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
