using Microsoft.EntityFrameworkCore;
using StudyTracker.Data;
using StudyTracker.Models;

namespace StudyTracker.Repositories;

public class CourseRepository : ICourseRepository
{
    private readonly ApplicationDbContext _context;

    public CourseRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Course>> GetAllAsync()
    {
        return await _context.Courses
            .Include(course => course.Tasks)
            .OrderBy(course => course.Name)
            .ToListAsync();
    }

    public async Task<List<Course>> GetForStudentAsync(string studentId)
    {
        return await _context.StudentCourses
            .Where(link => link.StudentId == studentId)
            .Include(link => link.Course!)
                .ThenInclude(course => course.Tasks)
            .Select(link => link.Course!)
            .OrderBy(course => course.Name)
            .ToListAsync();
    }

    public async Task<Course?> GetByIdAsync(int id)
    {
        return await _context.Courses
            .Include(course => course.Tasks)
            .FirstOrDefaultAsync(course => course.Id == id);
    }

    public async Task<Course?> GetWithTasksAsync(int id)
    {
        return await _context.Courses
            .Include(course => course.Tasks.OrderBy(task => task.Deadline))
            .Include(course => course.StudentCourses)
            .FirstOrDefaultAsync(course => course.Id == id);
    }

    public void Add(Course course)
    {
        _context.Courses.Add(course);
    }

    public void Remove(Course course)
    {
        _context.Courses.Remove(course);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
