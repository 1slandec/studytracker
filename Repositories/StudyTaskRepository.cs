using Microsoft.EntityFrameworkCore;
using StudyTracker.Data;
using StudyTracker.Models;

namespace StudyTracker.Repositories;

public class StudyTaskRepository : IStudyTaskRepository
{
    private const string SortByDeadlineAscending = "deadline_asc";
    private const string SortByDeadlineDescending = "deadline_desc";

    private readonly ApplicationDbContext _context;

    public StudyTaskRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<StudyTask?> GetByIdAsync(int id)
    {
        return await _context.StudyTasks
            .Include(task => task.Course)
            .Include(task => task.StudentStatuses)
            .FirstOrDefaultAsync(task => task.Id == id);
    }

    public async Task<List<StudyTask>> GetByCourseAsync(int courseId)
    {
        return await _context.StudyTasks
            .Include(task => task.Course)
            .Where(task => task.CourseId == courseId)
            .ToListAsync();
    }

    public async Task<List<StudyTaskWithEffectiveStatus>> GetByCourseForListAsync(
        int courseId,
        string? studentId,
        TaskStatusType? statusFilter,
        string? sortOrder)
    {
        if (string.IsNullOrWhiteSpace(studentId))
        {
            var tasks = _context.StudyTasks
                .Where(task => task.CourseId == courseId);

            if (statusFilter.HasValue)
            {
                tasks = tasks.Where(task => task.Status == statusFilter.Value);
            }

            tasks = sortOrder switch
            {
                SortByDeadlineAscending => tasks.OrderBy(task => task.Deadline),
                SortByDeadlineDescending => tasks.OrderByDescending(task => task.Deadline),
                _ => tasks
            };

            return await tasks
                .Select(task => new StudyTaskWithEffectiveStatus(task, task.Status))
                .ToListAsync();
        }

        var query =
            from task in _context.StudyTasks
            join studentStatus in _context.StudentTaskStatuses.Where(status => status.StudentId == studentId)
                on task.Id equals studentStatus.StudyTaskId into statusGroup
            from studentStatus in statusGroup.DefaultIfEmpty()
            where task.CourseId == courseId
            select new
            {
                Task = task,
                EffectiveStatus = studentStatus == null ? task.Status : studentStatus.Status
            };

        if (statusFilter.HasValue)
        {
            query = query.Where(row => row.EffectiveStatus == statusFilter.Value);
        }

        query = sortOrder switch
        {
            SortByDeadlineAscending => query.OrderBy(row => row.Task.Deadline),
            SortByDeadlineDescending => query.OrderByDescending(row => row.Task.Deadline),
            _ => query
        };

        return await query
            .Select(row => new StudyTaskWithEffectiveStatus(row.Task, row.EffectiveStatus))
            .ToListAsync();
    }

    public void Add(StudyTask task)
    {
        _context.StudyTasks.Add(task);
    }

    public void Remove(StudyTask task)
    {
        _context.StudyTasks.Remove(task);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
