using StudyTracker.Models;

namespace StudyTracker.Repositories;

public interface ICourseRepository
{
    Task<List<Course>> GetAllAsync();

    Task<List<Course>> GetForStudentAsync(string studentId);

    Task<Course?> GetByIdAsync(int id);

    Task<Course?> GetWithTasksAsync(int id);

    void Add(Course course);

    void Remove(Course course);

    Task SaveChangesAsync();
}
