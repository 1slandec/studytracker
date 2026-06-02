using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyTracker.Models;
using StudyTracker.Services;
using StudyTracker.Services.Exceptions;
using StudyTracker.ViewModelBuilders;
using StudyTracker.ViewModels;

namespace StudyTracker.Controllers;

[Authorize]
public class CoursesController : Controller
{
    private readonly ICourseService _courseService;
    private readonly IStudyTaskService _taskService;
    private readonly CourseViewModelBuilder _courseViewModelBuilder;
    private readonly StudyTaskViewModelBuilder _taskViewModelBuilder;

    public CoursesController(
        ICourseService courseService,
        IStudyTaskService taskService,
        CourseViewModelBuilder courseViewModelBuilder,
        StudyTaskViewModelBuilder taskViewModelBuilder)
    {
        _courseService = courseService;
        _taskService = taskService;
        _courseViewModelBuilder = courseViewModelBuilder;
        _taskViewModelBuilder = taskViewModelBuilder;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var courses = await _courseService.GetCoursesForUserAsync(User);
            var model = _courseViewModelBuilder.BuildList(courses, User.IsInRole(RoleNames.Administrator));

            return View(model);
        }
        catch (EntityNotFoundException exception)
        {
            TempData["ErrorMessage"] = exception.Message;
            return RedirectToAction("Index", "Home");
        }
    }

    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var taskResult = await _taskService.GetTasksForCourseAsync(
                id,
                User,
                null,
                StudyTaskService.SortByDeadlineAscending);

            var taskList = _taskViewModelBuilder.BuildList(
                taskResult,
                null,
                StudyTaskService.SortByDeadlineAscending);

            var model = _courseViewModelBuilder.BuildDetails(
                taskResult.Course,
                taskList.Tasks,
                User.IsInRole(RoleNames.Administrator),
                true);

            return View(model);
        }
        catch (EntityNotFoundException exception)
        {
            return NotFound(exception.Message);
        }
        catch (AccessDeniedException exception)
        {
            TempData["ErrorMessage"] = exception.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    [Authorize(Roles = RoleNames.Administrator)]
    public IActionResult Create()
    {
        return View(new CourseFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = RoleNames.Administrator)]
    public async Task<IActionResult> Create(CourseFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            await _courseService.CreateAsync(ToCourse(model));
            TempData["SuccessMessage"] = "Курс создан.";
            return RedirectToAction(nameof(Index));
        }
        catch (FormValidationException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return View(model);
        }
        catch (DataSaveException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return View(model);
        }
    }

    [Authorize(Roles = RoleNames.Administrator)]
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var course = await _courseService.GetCourseForAdminAsync(id);
            return View(_courseViewModelBuilder.BuildForm(course));
        }
        catch (EntityNotFoundException exception)
        {
            return NotFound(exception.Message);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = RoleNames.Administrator)]
    public async Task<IActionResult> Edit(int id, CourseFormViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest("Неверные данные формы.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            await _courseService.UpdateAsync(id, ToCourse(model));
            TempData["SuccessMessage"] = "Курс обновлен.";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (EntityNotFoundException exception)
        {
            return NotFound(exception.Message);
        }
        catch (FormValidationException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return View(model);
        }
        catch (DataSaveException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return View(model);
        }
    }

    [Authorize(Roles = RoleNames.Administrator)]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var course = await _courseService.GetCourseForAdminAsync(id);
            return View(_courseViewModelBuilder.BuildForm(course));
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
            await _courseService.DeleteAsync(id);
            TempData["SuccessMessage"] = "Курс удален.";
            return RedirectToAction(nameof(Index));
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

    [Authorize(Roles = RoleNames.Administrator)]
    public async Task<IActionResult> Assign(int id)
    {
        try
        {
            var data = await _courseService.GetAssignmentDataAsync(id);
            return View(_courseViewModelBuilder.BuildAssignment(data.Course, data.Students, data.AssignedStudentIds));
        }
        catch (EntityNotFoundException exception)
        {
            return NotFound(exception.Message);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = RoleNames.Administrator)]
    public async Task<IActionResult> Assign(int id, AssignCourseViewModel model)
    {
        if (id != model.CourseId)
        {
            return BadRequest("Неверные данные формы.");
        }

        try
        {
            await _courseService.AssignStudentsAsync(id, model.SelectedStudentIds);
            TempData["SuccessMessage"] = "Назначения студентов обновлены.";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (EntityNotFoundException exception)
        {
            return NotFound(exception.Message);
        }
        catch (DataSaveException exception)
        {
            TempData["ErrorMessage"] = exception.Message;
            var data = await _courseService.GetAssignmentDataAsync(id);
            return View(_courseViewModelBuilder.BuildAssignment(data.Course, data.Students, data.AssignedStudentIds));
        }
    }

    private static Course ToCourse(CourseFormViewModel model)
    {
        return new Course
        {
            Id = model.Id,
            Name = model.Name,
            Description = model.Description,
            ProfessorName = model.ProfessorName
        };
    }
}
