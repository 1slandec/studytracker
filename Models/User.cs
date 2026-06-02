using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace StudyTracker.Models;

public class User : IdentityUser
{
    [Required(ErrorMessage = "Укажите ФИО пользователя.")]
    [StringLength(120)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    public UserRole Role { get; set; } = UserRole.Student;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<StudentCourse> StudentCourses { get; set; } = new List<StudentCourse>();

    public ICollection<StudentTaskStatus> TaskStatuses { get; set; } = new List<StudentTaskStatus>();
}
