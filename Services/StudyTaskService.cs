using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudyTracker.Models;
using StudyTracker.Repositories;
using StudyTracker.Services.Exceptions;

namespace StudyTracker.Services;

public class StudyTaskService : IStudyTaskService
{
    public const string SortByDeadlineAscending = "deadline_asc";
    public const string SortByDeadlineDescending = "deadline_desc";

    private readonly ICourseService _courseService;
    private readonly IStudyTaskRepository _taskRepository;
    private readonly IStudentCourseRepository _studentCourseRepository;
    private readonly IStudentTaskStatusRepository _statusRepository;
    private readonly IUserRepository _userRepository;
    private readonly UserManager<User> _userManager;

    public StudyTaskService(
        ICourseService courseService,
        IStudyTaskRepository taskRepository,
        IStudentCourseRepository studentCourseRepository,
        IStudentTaskStatusRepository statusRepository,
        IUserRepository userRepository,
        UserManager<User> userManager)
    {
        _courseService = courseService;
        _taskRepository = taskRepository;
        _studentCourseRepository = studentCourseRepository;
        _statusRepository = statusRepository;
        _userRepository = userRepository;
        _userManager = userManager;
    }

    public async Task<StudyTaskListResult> GetTasksForCourseAsync(
        int courseId,
        ClaimsPrincipal user,
        TaskStatusType? statusFilter,
        string? sortOrder)
    {
        var course = await _courseService.GetCourseForUserAsync(courseId, user);
        var studentId = user.IsInRole(RoleNames.Student)
            ? await GetExistingUserIdAsync(user)
            : null;

        var taskRows = await _taskRepository.GetByCourseForListAsync(
            courseId,
            studentId,
            statusFilter,
            sortOrder);
        var tasks = taskRows.Select(row => row.Task).ToList();
        var effectiveStatuses = taskRows.ToDictionary(row => row.Task.Id, row => row.EffectiveStatus);

        return new StudyTaskListResult(
            course,
            tasks,
            effectiveStatuses,
            user.IsInRole(RoleNames.Administrator),
            user.IsInRole(RoleNames.Student));
    }

    public async Task<StudyTaskDetailsResult> GetTaskForUserAsync(int id, ClaimsPrincipal user)
    {
        var task = await _taskRepository.GetByIdAsync(id)
            ?? throw new EntityNotFoundException("Задание не найдено.");

        _ = await _courseService.GetCourseForUserAsync(task.CourseId, user);

        var status = task.Status;
        if (user.IsInRole(RoleNames.Student))
        {
            var studentId = await GetExistingUserIdAsync(user);
            status = (await _statusRepository.GetAsync(studentId, task.Id))?.Status ?? task.Status;
        }

        return new StudyTaskDetailsResult(
            task,
            status,
            user.IsInRole(RoleNames.Administrator),
            user.IsInRole(RoleNames.Student));
    }

    public async Task<StudyTask> GetTaskForAdminAsync(int id)
    {
        return await _taskRepository.GetByIdAsync(id)
            ?? throw new EntityNotFoundException("Задание не найдено.");
    }

    public async Task<StudyTask> CreateAsync(int courseId, StudyTask task)
    {
        _ = await _courseService.GetCourseForAdminAsync(courseId);
        ValidateTask(task);
        task.CourseId = courseId;

        try
        {
            _taskRepository.Add(task);
            await _taskRepository.SaveChangesAsync();
            return task;
        }
        catch (DbUpdateException exception)
        {
            throw new DataSaveException("Ошибка при сохранении задания.", exception);
        }
    }

    public async Task UpdateAsync(int id, StudyTask values)
    {
        ValidateTask(values);

        var task = await _taskRepository.GetByIdAsync(id)
            ?? throw new EntityNotFoundException("Задание не найдено.");

        task.Title = values.Title.Trim();
        task.Description = values.Description.Trim();
        task.Deadline = values.Deadline.Date;
        task.Status = values.Status;

        try
        {
            await _taskRepository.SaveChangesAsync();
        }
        catch (DbUpdateException exception)
        {
            throw new DataSaveException("Ошибка при сохранении задания.", exception);
        }
    }

    public async Task DeleteAsync(int id)
    {
        var task = await _taskRepository.GetByIdAsync(id)
            ?? throw new EntityNotFoundException("Задание не найдено.");

        try
        {
            _taskRepository.Remove(task);
            await _taskRepository.SaveChangesAsync();
        }
        catch (DbUpdateException exception)
        {
            throw new DataSaveException("Ошибка при удалении задания.", exception);
        }
    }

    public async Task ChangeStudentStatusAsync(int taskId, ClaimsPrincipal user, TaskStatusType status)
    {
        if (!user.IsInRole(RoleNames.Student))
        {
            throw new AccessDeniedException("Только студент может менять личный статус задания.");
        }

        var studentId = await GetExistingUserIdAsync(user);
        var task = await _taskRepository.GetByIdAsync(taskId)
            ?? throw new EntityNotFoundException("Задание не найдено.");

        var isAssigned = await _studentCourseRepository.IsStudentAssignedAsync(studentId, task.CourseId);
        if (!isAssigned)
        {
            throw new AccessDeniedException("У вас нет доступа к этому заданию.");
        }

        try
        {
            await _statusRepository.UpsertAsync(studentId, taskId, status);
        }
        catch (DbUpdateException exception)
        {
            throw new DataSaveException("Ошибка при сохранении статуса задания.", exception);
        }
    }

    private async Task<string> GetExistingUserIdAsync(ClaimsPrincipal user)
    {
        var userId = _userManager.GetUserId(user);
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new EntityNotFoundException("Пользователь не найден.");
        }

        var existingUser = await _userRepository.GetByIdAsync(userId);
        if (existingUser is null)
        {
            throw new EntityNotFoundException("Пользователь не найден.");
        }

        return userId;
    }

    private static void ValidateTask(StudyTask task)
    {
        if (string.IsNullOrWhiteSpace(task.Title) || string.IsNullOrWhiteSpace(task.Description))
        {
            throw new FormValidationException("Заполните все обязательные поля задания.");
        }

        task.Title = task.Title.Trim();
        task.Description = task.Description.Trim();
        task.Deadline = task.Deadline.Date;
    }
}
