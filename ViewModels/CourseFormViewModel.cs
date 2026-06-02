using System.ComponentModel.DataAnnotations;

namespace StudyTracker.ViewModels;

public class CourseFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Введите название курса.")]
    [StringLength(120)]
    [Display(Name = "Название")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите описание курса.")]
    [StringLength(1000)]
    [Display(Name = "Описание")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите имя преподавателя.")]
    [StringLength(120)]
    [Display(Name = "Преподаватель")]
    public string ProfessorName { get; set; } = string.Empty;
}
