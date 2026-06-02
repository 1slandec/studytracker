using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyTracker.Models;
using StudyTracker.Services;
using StudyTracker.Services.Exceptions;
using StudyTracker.ViewModelBuilders;
using StudyTracker.ViewModels;

namespace StudyTracker.Controllers;

[Authorize]
public class TasksController : Controller
{
    private readonly ICourseService _courseService;
    private readonly IStudyTaskService _taskService;
    private readonly StudyTaskViewModelBuilder _taskViewModelBuilder;

    public TasksController(
        ICourseService courseService,
        IStudyTaskService taskService,
        StudyTaskViewModelBuilder taskViewModelBuilder)
    {
        _courseService = courseService;
        _taskService = taskService;
        _taskViewModelBuilder = taskViewModelBuilder;
    }

    public async Task<IActionResult> Index(int courseId, TaskStatusType? statusFilter, string? sortOrder)
    {
        try
        {
            var result = await _taskService.GetTasksForCourseAsync(courseId, User, statusFilter, sortOrder);
            return View(_taskViewModelBuilder.BuildList(result, statusFilter, sortOrder));
        }
        catch (EntityNotFoundException exception)
        {
            return NotFound(exception.Message);
        }
        catch (AccessDeniedException exception)
        {
            TempData["ErrorMessage"] = exception.Message;
            return RedirectToAction("Index", "Courses");
        }
    }

    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var result = await _taskService.GetTaskForUserAsync(id, User);
            return View(_taskViewModelBuilder.BuildDetails(result));
        }
        catch (EntityNotFoundException exception)
        {
            return NotFound(exception.Message);
        }
        catch (AccessDeniedException exception)
        {
            TempData["ErrorMessage"] = exception.Message;
            return RedirectToAction("Index", "Courses");
        }
    }

    [Authorize(Roles = RoleNames.Administrator)]
    public async Task<IActionResult> Create(int courseId)
    {
        try
        {
            var course = await _courseService.GetCourseForAdminAsync(courseId);
            return View(new StudyTaskFormViewModel
            {
                CourseId = course.Id,
                CourseName = course.Name
            });
        }
        catch (EntityNotFoundException exception)
        {
            return NotFound(exception.Message);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = RoleNames.Administrator)]
    public async Task<IActionResult> Create(StudyTaskFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await FillCourseNameAsync(model);
            return View(model);
        }

        try
        {
            var task = await _taskService.CreateAsync(model.CourseId, ToTask(model));
            TempData["SuccessMessage"] = "Задание создано.";
            return RedirectToAction(nameof(Details), new { id = task.Id });
        }
        catch (EntityNotFoundException exception)
        {
            return NotFound(exception.Message);
        }
        catch (FormValidationException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            await FillCourseNameAsync(model);
            return View(model);
        }
        catch (DataSaveException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            await FillCourseNameAsync(model);
            return View(model);
        }
    }

    [Authorize(Roles = RoleNames.Administrator)]
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var task = await _taskService.GetTaskForAdminAsync(id);
            return View(_taskViewModelBuilder.BuildForm(task));
        }
        catch (EntityNotFoundException exception)
        {
            return NotFound(exception.Message);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = RoleNames.Administrator)]
    public async Task<IActionResult> Edit(int id, StudyTaskFormViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest("Неверные данные формы.");
        }

        if (!ModelState.IsValid)
        {
            await FillCourseNameAsync(model);
            return View(model);
        }

        try
        {
            await _taskService.UpdateAsync(id, ToTask(model));
            TempData["SuccessMessage"] = "Задание обновлено.";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (EntityNotFoundException exception)
        {
            return NotFound(exception.Message);
        }
        catch (FormValidationException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            await FillCourseNameAsync(model);
            return View(model);
        }
        catch (DataSaveException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            await FillCourseNameAsync(model);
            return View(model);
        }
    }

    [Authorize(Roles = RoleNames.Administrator)]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var task = await _taskService.GetTaskForAdminAsync(id);
            return View(_taskViewModelBuilder.BuildForm(task));
        }
        catch (EntityNotFoundException exception)
        {
            return NotFound(exception.Message);
        }
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = RoleNames.Administrator)]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            var task = await _taskService.GetTaskForAdminAsync(id);
            var courseId = task.CourseId;
            await _taskService.DeleteAsync(id);
            TempData["SuccessMessage"] = "Задание удалено.";
            return RedirectToAction(nameof(Index), new { courseId });
        }
        catch (EntityNotFoundException exception)
        {
            return NotFound(exception.Message);
        }
        catch (DataSaveException exception)
        {
            TempData["ErrorMessage"] = exception.Message;
            return RedirectToAction(nameof(Delete), new { id });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = RoleNames.Student)]
    public async Task<IActionResult> ChangeStatus(ChangeTaskStatusViewModel model)
    {
        try
        {
            await _taskService.ChangeStudentStatusAsync(model.TaskId, User, model.Status);
            TempData["SuccessMessage"] = "Статус задания обновлен.";

            if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return LocalRedirect(model.ReturnUrl);
            }

            return RedirectToAction(nameof(Details), new { id = model.TaskId });
        }
        catch (EntityNotFoundException exception)
        {
            return NotFound(exception.Message);
        }
        catch (AccessDeniedException exception)
        {
            TempData["ErrorMessage"] = exception.Message;
            return RedirectToAction("Index", "Courses");
        }
        catch (DataSaveException exception)
        {
            TempData["ErrorMessage"] = exception.Message;
            return RedirectToAction(nameof(Details), new { id = model.TaskId });
        }
    }

    private async Task FillCourseNameAsync(StudyTaskFormViewModel model)
    {
        if (model.CourseId == 0)
        {
            return;
        }

        try
        {
            var course = await _courseService.GetCourseForAdminAsync(model.CourseId);
            model.CourseName = course.Name;
        }
        catch (EntityNotFoundException)
        {
            model.CourseName = string.Empty;
        }
    }

    private static StudyTask ToTask(StudyTaskFormViewModel model)
    {
        return new StudyTask
        {
            Id = model.Id,
            CourseId = model.CourseId,
            Title = model.Title,
            Description = model.Description,
            Deadline = model.Deadline,
            Status = model.Status
        };
    }
}
