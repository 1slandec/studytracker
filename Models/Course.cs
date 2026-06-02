using System.ComponentModel.DataAnnotations;

namespace StudyTracker.Models;

public class Course
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Введите название курса.")]
    [StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите описание курса.")]
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите имя преподавателя.")]
    [Display(Name = "Преподаватель")]
    [StringLength(120)]
    public string ProfessorName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<StudyTask> Tasks { get; set; } = new List<StudyTask>();

    public ICollection<StudentCourse> StudentCourses { get; set; } = new List<StudentCourse>();
}
