using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudyTracker.Models;

namespace StudyTracker.Data;

public static class SeedData
{
    public const string AdminEmail = "admin@studytracker.local";
    public const string StudentEmail = "student@studytracker.local";
    public const string SecondStudentEmail = "ivan@studytracker.local";
    public const string DefaultPassword = "Study123!";

    public static async Task InitializeAsync(IServiceProvider services)
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<User>>();

        await EnsureRoleAsync(roleManager, RoleNames.Administrator);
        await EnsureRoleAsync(roleManager, RoleNames.Student);

        var admin = await EnsureUserAsync(
            userManager,
            AdminEmail,
            "Администратор StudyTracker",
            UserRole.Administrator);

        var student = await EnsureUserAsync(
            userManager,
            StudentEmail,
            "Анна Смирнова",
            UserRole.Student);

        var secondStudent = await EnsureUserAsync(
            userManager,
            SecondStudentEmail,
            "Иван Петров",
            UserRole.Student);

        if (!await context.Courses.AnyAsync())
        {
            var programming = new Course
            {
                Name = "Основы программирования",
                Description = "Базовый курс по C#, алгоритмам и структурам данных.",
                ProfessorName = "Проф. Елена Волкова",
                Tasks =
                {
                    new StudyTask
                    {
                        Title = "Лабораторная работа 1",
                        Description = "Реализовать консольное приложение для обработки массива.",
                        Deadline = DateTime.Today.AddDays(5),
                        Status = TaskStatusType.NotStarted
                    },
                    new StudyTask
                    {
                        Title = "Практика по LINQ",
                        Description = "Подготовить выборки, сортировки и группировки коллекций.",
                        Deadline = DateTime.Today.AddDays(12),
                        Status = TaskStatusType.NotStarted
                    }
                }
            };

            var databases = new Course
            {
                Name = "Базы данных",
                Description = "Проектирование схем, SQL-запросы и основы транзакций.",
                ProfessorName = "Доц. Сергей Орлов",
                Tasks =
                {
                    new StudyTask
                    {
                        Title = "ER-диаграмма",
                        Description = "Спроектировать ER-модель для учебного проекта.",
                        Deadline = DateTime.Today.AddDays(-2),
                        Status = TaskStatusType.NotStarted
                    },
                    new StudyTask
                    {
                        Title = "SQL-запросы",
                        Description = "Написать запросы SELECT, JOIN, GROUP BY и HAVING.",
                        Deadline = DateTime.Today.AddDays(7),
                        Status = TaskStatusType.NotStarted
                    }
                }
            };

            context.Courses.AddRange(programming, databases);
            await context.SaveChangesAsync();

            context.StudentCourses.AddRange(
                new StudentCourse { StudentId = student.Id, CourseId = programming.Id },
                new StudentCourse { StudentId = student.Id, CourseId = databases.Id },
                new StudentCourse { StudentId = secondStudent.Id, CourseId = databases.Id });

            await context.SaveChangesAsync();

            var linqTask = programming.Tasks.Single(task => task.Title == "Практика по LINQ");
            var erTask = databases.Tasks.Single(task => task.Title == "ER-диаграмма");

            context.StudentTaskStatuses.AddRange(
                new StudentTaskStatus
                {
                    StudentId = student.Id,
                    StudyTaskId = linqTask.Id,
                    Status = TaskStatusType.InProgress
                },
                new StudentTaskStatus
                {
                    StudentId = secondStudent.Id,
                    StudyTaskId = erTask.Id,
                    Status = TaskStatusType.NotStarted
                });

            await context.SaveChangesAsync();
        }
    }

    private static async Task EnsureRoleAsync(RoleManager<IdentityRole> roleManager, string roleName)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    private static async Task<User> EnsureUserAsync(
        UserManager<User> userManager,
        string email,
        string fullName,
        UserRole role)
    {
        var roleName = role.ToString();
        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
        {
            user = new User
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FullName = fullName,
                Role = role
            };

            var createResult = await userManager.CreateAsync(user, DefaultPassword);
            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Не удалось создать пользователя {email}: {string.Join("; ", createResult.Errors.Select(error => error.Description))}");
            }
        }
        else if (user.FullName != fullName || user.Role != role)
        {
            user.FullName = fullName;
            user.Role = role;
            await userManager.UpdateAsync(user);
        }

        if (!await userManager.IsInRoleAsync(user, roleName))
        {
            await userManager.AddToRoleAsync(user, roleName);
        }

        return user;
    }
}
