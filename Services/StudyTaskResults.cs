using StudyTracker.Models;

namespace StudyTracker.Services;

public sealed record StudyTaskListResult(
    Course Course,
    IReadOnlyList<StudyTask> Tasks,
    IReadOnlyDictionary<int, TaskStatusType> EffectiveStatuses,
    bool CanManageTasks,
    bool CanChangeStatus);

public sealed record StudyTaskDetailsResult(
    StudyTask Task,
    TaskStatusType EffectiveStatus,
    bool CanManageTasks,
    bool CanChangeStatus);
