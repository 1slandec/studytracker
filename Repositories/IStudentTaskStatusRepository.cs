using StudyTracker.Models;

namespace StudyTracker.Repositories;

public interface IStudentTaskStatusRepository
{
    Task<StudentTaskStatus?> GetAsync(string studentId, int taskId);

    Task<List<StudentTaskStatus>> GetForStudentByCourseAsync(string studentId, int courseId);

    Task UpsertAsync(string studentId, int taskId, TaskStatusType status);
}
