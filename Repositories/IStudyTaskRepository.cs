using StudyTracker.Models;

namespace StudyTracker.Repositories;

public interface IStudyTaskRepository
{
    Task<StudyTask?> GetByIdAsync(int id);

    Task<List<StudyTask>> GetByCourseAsync(int courseId);

    Task<List<StudyTaskWithEffectiveStatus>> GetByCourseForListAsync(
        int courseId,
        string? studentId,
        TaskStatusType? statusFilter,
        string? sortOrder);

    void Add(StudyTask task);

    void Remove(StudyTask task);

    Task SaveChangesAsync();
}
