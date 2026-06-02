using System.ComponentModel.DataAnnotations;

namespace StudyTracker.Models;

public class StudyTask
{
    public int Id { get; set; }

    [Required]
    public int CourseId { get; set; }

    public Course? Course { get; set; }

    [Required(ErrorMessage = "Введите название задания.")]
    [StringLength(160)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите описание задания.")]
    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажите дедлайн.")]
    [DataType(DataType.Date)]
    public DateTime Deadline { get; set; } = DateTime.Today.AddDays(7);

    [Required]
    public TaskStatusType Status { get; set; } = TaskStatusType.NotStarted;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<StudentTaskStatus> StudentStatuses { get; set; } = new List<StudentTaskStatus>();
}
