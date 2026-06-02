using Microsoft.EntityFrameworkCore;
using StudyTracker.Data;
using StudyTracker.Models;

namespace StudyTracker.Repositories;

public class StudentCourseRepository : IStudentCourseRepository
{
    private readonly ApplicationDbContext _context;

    public StudentCourseRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> IsStudentAssignedAsync(string studentId, int courseId)
    {
        return await _context.StudentCourses
            .AnyAsync(link => link.StudentId == studentId && link.CourseId == courseId);
    }

    public async Task<List<string>> GetStudentIdsForCourseAsync(int courseId)
    {
        return await _context.StudentCourses
            .Where(link => link.CourseId == courseId)
            .Select(link => link.StudentId)
            .ToListAsync();
    }

    public async Task SetAssignedStudentsAsync(int courseId, IEnumerable<string> selectedStudentIds)
    {
        var selected = selectedStudentIds.ToHashSet();
        var existing = await _context.StudentCourses
            .Where(link => link.CourseId == courseId)
            .ToListAsync();

        var linksToRemove = existing
            .Where(link => !selected.Contains(link.StudentId))
            .ToList();

        var existingIds = existing.Select(link => link.StudentId).ToHashSet();
        var linksToAdd = selected
            .Where(studentId => !existingIds.Contains(studentId))
            .Select(studentId => new StudentCourse
            {
                CourseId = courseId,
                StudentId = studentId
            });

        _context.StudentCourses.RemoveRange(linksToRemove);
        _context.StudentCourses.AddRange(linksToAdd);
        await _context.SaveChangesAsync();
    }
}
