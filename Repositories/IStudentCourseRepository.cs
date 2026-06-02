namespace StudyTracker.Repositories;

public interface IStudentCourseRepository
{
    Task<bool> IsStudentAssignedAsync(string studentId, int courseId);

    Task<List<string>> GetStudentIdsForCourseAsync(int courseId);

    Task SetAssignedStudentsAsync(int courseId, IEnumerable<string> selectedStudentIds);
}
