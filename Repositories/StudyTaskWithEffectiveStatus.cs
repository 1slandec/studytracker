using StudyTracker.Models;

namespace StudyTracker.Repositories;

public sealed record StudyTaskWithEffectiveStatus(
    StudyTask Task,
    TaskStatusType EffectiveStatus);
