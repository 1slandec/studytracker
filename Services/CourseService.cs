using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudyTracker.Models;
using StudyTracker.Repositories;
using StudyTracker.Services.Exceptions;

namespace StudyTracker.Services;

public class CourseService : ICourseService
{
    private readonly ICourseRepository _courseRepository;
    private readonly IStudentCourseRepository _studentCourseRepository;
    private readonly IUserRepository _userRepository;
    private readonly UserManager<User> _userManager;

    public CourseService(
        ICourseRepository courseRepository,
        IStudentCourseRepository studentCourseRepository,
        IUserRepository userRepository,
        UserManager<User> userManager)
    {
        _courseRepository = courseRepository;
        _studentCourseRepository = studentCourseRepository;
        _userRepository = userRepository;
        _userManager = userManager;
    }

    public async Task<List<Course>> GetCoursesForUserAsync(ClaimsPrincipal user)
    {
        if (user.IsInRole(RoleNames.Administrator))
        {
            return await _courseRepository.GetAllAsync();
        }

        var studentId = await GetExistingUserIdAsync(user);
        return await _courseRepository.GetForStudentAsync(studentId);
    }

    public async Task<Course> GetCourseForUserAsync(int id, ClaimsPrincipal user)
    {
        var course = await _courseRepository.GetWithTasksAsync(id)
            ?? throw new EntityNotFoundException("Курс не найден.");

        if (user.IsInRole(RoleNames.Administrator))
        {
            return course;
        }

        var studentId = await GetExistingUserIdAsync(user);
        var isAssigned = await _studentCourseRepository.IsStudentAssignedAsync(studentId, id);

        if (!isAssigned)
        {
            throw new AccessDeniedException("У вас нет доступа к этому курсу.");
        }

        return course;
    }

    public async Task<Course> GetCourseForAdminAsync(int id)
    {
        return await _courseRepository.GetWithTasksAsync(id)
            ?? throw new EntityNotFoundException("Курс не найден.");
    }

    public async Task<Course> CreateAsync(Course course)
    {
        ValidateCourse(course);

        try
        {
            _courseRepository.Add(course);
            await _courseRepository.SaveChangesAsync();
            return course;
        }
        catch (DbUpdateException exception)
        {
            throw new DataSaveException("Ошибка при сохранении курса.", exception);
        }
    }

    public async Task UpdateAsync(int id, Course values)
    {
        ValidateCourse(values);

        var course = await _courseRepository.GetByIdAsync(id)
            ?? throw new EntityNotFoundException("Курс не найден.");

        course.Name = values.Name.Trim();
        course.Description = values.Description.Trim();
        course.ProfessorName = values.ProfessorName.Trim();

        try
        {
            await _courseRepository.SaveChangesAsync();
        }
        catch (DbUpdateException exception)
        {
            throw new DataSaveException("Ошибка при сохранении курса.", exception);
        }
    }

    public async Task DeleteAsync(int id)
    {
        var course = await _courseRepository.GetByIdAsync(id)
            ?? throw new EntityNotFoundException("Курс не найден.");

        try
        {
            _courseRepository.Remove(course);
            await _courseRepository.SaveChangesAsync();
        }
        catch (DbUpdateException exception)
        {
            throw new DataSaveException("Ошибка при удалении курса.", exception);
        }
    }

    public async Task<CourseAssignmentData> GetAssignmentDataAsync(int courseId)
    {
        var course = await GetCourseForAdminAsync(courseId);
        var students = await _userRepository.GetStudentsAsync();
        var assignedStudentIds = await _studentCourseRepository.GetStudentIdsForCourseAsync(courseId);

        return new CourseAssignmentData(course, students, assignedStudentIds);
    }

    public async Task AssignStudentsAsync(int courseId, IEnumerable<string> studentIds)
    {
        _ = await GetCourseForAdminAsync(courseId);
        var students = await _userRepository.GetStudentsAsync();
        var knownStudentIds = students.Select(student => student.Id).ToHashSet();
        var selected = studentIds.Where(knownStudentIds.Contains).Distinct().ToArray();

        try
        {
            await _studentCourseRepository.SetAssignedStudentsAsync(courseId, selected);
        }
        catch (DbUpdateException exception)
        {
            throw new DataSaveException("Ошибка при назначении студентов на курс.", exception);
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

    private static void ValidateCourse(Course course)
    {
        if (string.IsNullOrWhiteSpace(course.Name)
            || string.IsNullOrWhiteSpace(course.Description)
            || string.IsNullOrWhiteSpace(course.ProfessorName))
        {
            throw new FormValidationException("Заполните все обязательные поля курса.");
        }

        course.Name = course.Name.Trim();
        course.Description = course.Description.Trim();
        course.ProfessorName = course.ProfessorName.Trim();
    }
}
