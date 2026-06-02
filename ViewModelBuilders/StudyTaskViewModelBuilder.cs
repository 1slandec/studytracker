using StudyTracker.Models;
using StudyTracker.Services;
using StudyTracker.ViewModels;

namespace StudyTracker.ViewModelBuilders;

public class StudyTaskViewModelBuilder
{
    public StudyTaskListViewModel BuildList(
        StudyTaskListResult result,
        TaskStatusType? statusFilter,
        string? sortOrder)
    {
        return new StudyTaskListViewModel
        {
            CourseId = result.Course.Id,
            CourseName = result.Course.Name,
            StatusFilter = statusFilter,
            SortOrder = sortOrder ?? string.Empty,
            CanManageTasks = result.CanManageTasks,
            CanChangeStatus = result.CanChangeStatus,
            Tasks = result.Tasks.Select(task => BuildListItem(
                    task,
                    result.EffectiveStatuses.TryGetValue(task.Id, out var status) ? status : task.Status,
                    result.CanManageTasks,
                    result.CanChangeStatus))
                .ToList()
        };
    }

    public StudyTaskDetailsViewModel BuildDetails(StudyTaskDetailsResult result)
    {
        return new StudyTaskDetailsViewModel
        {
            Id = result.Task.Id,
            CourseId = result.Task.CourseId,
            CourseName = result.Task.Course?.Name ?? string.Empty,
            Title = result.Task.Title,
            Description = result.Task.Description,
            Deadline = result.Task.Deadline,
            Status = result.EffectiveStatus,
            StatusName = result.EffectiveStatus.ToDisplayName(),
            IsOverdue = IsOverdue(result.Task.Deadline, result.EffectiveStatus),
            CanManageTask = result.CanManageTasks,
            CanChangeStatus = result.CanChangeStatus
        };
    }

    public StudyTaskFormViewModel BuildForm(StudyTask task)
    {
        return new StudyTaskFormViewModel
        {
            Id = task.Id,
            CourseId = task.CourseId,
            CourseName = task.Course?.Name ?? string.Empty,
            Title = task.Title,
            Description = task.Description,
            Deadline = task.Deadline,
            Status = task.Status
        };
    }

    public StudyTaskListItemViewModel BuildListItem(
        StudyTask task,
        TaskStatusType effectiveStatus,
        bool canManageTask,
        bool canChangeStatus)
    {
        return new StudyTaskListItemViewModel
        {
            Id = task.Id,
            CourseId = task.CourseId,
            CourseName = task.Course?.Name ?? string.Empty,
            Title = task.Title,
            Description = task.Description,
            Deadline = task.Deadline,
            Status = effectiveStatus,
            StatusName = effectiveStatus.ToDisplayName(),
            IsOverdue = IsOverdue(task.Deadline, effectiveStatus),
            CanManageTask = canManageTask,
            CanChangeStatus = canChangeStatus
        };
    }

    private static bool IsOverdue(DateTime deadline, TaskStatusType status)
    {
        return deadline.Date < DateTime.Today && status != TaskStatusType.Completed;
    }
}
