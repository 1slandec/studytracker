using System.ComponentModel.DataAnnotations;

namespace StudyTracker.Models;

public class StudentCourse
{
    public int Id { get; set; }

    [Required]
    public string StudentId { get; set; } = string.Empty;

    public User? Student { get; set; }

    [Required]
    public int CourseId { get; set; }

    public Course? Course { get; set; }

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
}
