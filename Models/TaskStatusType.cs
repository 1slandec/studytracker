using System.ComponentModel.DataAnnotations;

namespace StudyTracker.Models;

public enum TaskStatusType
{
    [Display(Name = "Не начато")]
    NotStarted = 0,

    [Display(Name = "В процессе")]
    InProgress = 1,

    [Display(Name = "Завершено")]
    Completed = 2
}
