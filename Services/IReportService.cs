using System.Security.Claims;

namespace StudyTracker.Services;

public interface IReportService
{
    Task<ReportFile> BuildStudentTasksReportAsync(int courseId, ClaimsPrincipal user, string format);

    Task<ReportFile> BuildOverdueStudentsReportAsync(string format);
}
