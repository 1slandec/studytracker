using System.ComponentModel.DataAnnotations;
using StudyTracker.Models;

namespace StudyTracker.ViewModels;

public class StudyTaskFormViewModel
{
    public int Id { get; set; }

    public int CourseId { get; set; }

    public string CourseName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите название задания.")]
    [StringLength(160)]
    [Display(Name = "Название")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите описание задания.")]
    [StringLength(2000)]
    [Display(Name = "Описание")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажите дедлайн.")]
    [DataType(DataType.Date)]
    [Display(Name = "Дедлайн")]
    public DateTime Deadline { get; set; } = DateTime.Today.AddDays(7);

    [Display(Name = "Статус по умолчанию")]
    public TaskStatusType Status { get; set; } = TaskStatusType.NotStarted;
}
