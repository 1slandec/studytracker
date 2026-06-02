using System.Security.Claims;
using StudyTracker.Models;

namespace StudyTracker.Services;

public interface ICourseService
{
    Task<List<Course>> GetCoursesForUserAsync(ClaimsPrincipal user);

    Task<Course> GetCourseForUserAsync(int id, ClaimsPrincipal user);

    Task<Course> GetCourseForAdminAsync(int id);

    Task<Course> CreateAsync(Course course);

    Task UpdateAsync(int id, Course values);

    Task DeleteAsync(int id);

    Task<CourseAssignmentData> GetAssignmentDataAsync(int courseId);

    Task AssignStudentsAsync(int courseId, IEnumerable<string> studentIds);
}
