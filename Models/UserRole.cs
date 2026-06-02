using System.ComponentModel.DataAnnotations;

namespace StudyTracker.Models;

public enum UserRole
{
    [Display(Name = "Студент")]
    Student = 0,

    [Display(Name = "Администратор")]
    Administrator = 1
}
