using System.Security.Claims;
using StudyTracker.Models;

namespace StudyTracker.Services;

public interface IStudyTaskService
{
    Task<StudyTaskListResult> GetTasksForCourseAsync(
        int courseId,
        ClaimsPrincipal user,
        TaskStatusType? statusFilter,
        string? sortOrder);

    Task<StudyTaskDetailsResult> GetTaskForUserAsync(int id, ClaimsPrincipal user);

    Task<StudyTask> GetTaskForAdminAsync(int id);

    Task<StudyTask> CreateAsync(int courseId, StudyTask task);

    Task UpdateAsync(int id, StudyTask values);

    Task DeleteAsync(int id);

    Task ChangeStudentStatusAsync(int taskId, ClaimsPrincipal user, TaskStatusType status);
}
