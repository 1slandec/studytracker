using Microsoft.EntityFrameworkCore;
using StudyTracker.Data;
using StudyTracker.Models;

namespace StudyTracker.Repositories;

public class StudentTaskStatusRepository : IStudentTaskStatusRepository
{
    private readonly ApplicationDbContext _context;

    public StudentTaskStatusRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<StudentTaskStatus?> GetAsync(string studentId, int taskId)
    {
        return await _context.StudentTaskStatuses
            .FirstOrDefaultAsync(status => status.StudentId == studentId && status.StudyTaskId == taskId);
    }

    public async Task<List<StudentTaskStatus>> GetForStudentByCourseAsync(string studentId, int courseId)
    {
        return await _context.StudentTaskStatuses
            .Include(status => status.StudyTask)
            .Where(status => status.StudentId == studentId && status.StudyTask!.CourseId == courseId)
            .ToListAsync();
    }

    public async Task UpsertAsync(string studentId, int taskId, TaskStatusType status)
    {
        var studentTaskStatus = await GetAsync(studentId, taskId);

        if (studentTaskStatus is null)
        {
            _context.StudentTaskStatuses.Add(new StudentTaskStatus
            {
                StudentId = studentId,
                StudyTaskId = taskId,
                Status = status,
                UpdatedAt = DateTime.UtcNow
            });
        }
        else
        {
            studentTaskStatus.Status = status;
            studentTaskStatus.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }
}
