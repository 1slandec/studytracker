using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyTracker.Models;
using StudyTracker.Services;
using StudyTracker.Services.Exceptions;

namespace StudyTracker.Controllers;

[Authorize]
public class ReportsController : Controller
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    public async Task<IActionResult> CourseTasks(int courseId, string format = "docx")
    {
        try
        {
            var report = await _reportService.BuildStudentTasksReportAsync(courseId, User, format);
            return File(report.Content, report.ContentType, report.FileName);
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
        catch (FormValidationException exception)
        {
            TempData["ErrorMessage"] = exception.Message;
            return RedirectToAction("Details", "Courses", new { id = courseId });
        }
    }

    [Authorize(Roles = RoleNames.Administrator)]
    public async Task<IActionResult> OverdueStudents(string format = "docx")
    {
        try
        {
            var report = await _reportService.BuildOverdueStudentsReportAsync(format);
            return File(report.Content, report.ContentType, report.FileName);
        }
        catch (FormValidationException exception)
        {
            TempData["ErrorMessage"] = exception.Message;
            return RedirectToAction("Index", "Courses");
        }
    }
}
