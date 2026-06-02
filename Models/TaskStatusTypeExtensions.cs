using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace StudyTracker.Models;

public static class TaskStatusTypeExtensions
{
    public static string ToDisplayName(this TaskStatusType status)
    {
        var field = typeof(TaskStatusType).GetField(status.ToString());
        var display = field?.GetCustomAttribute<DisplayAttribute>();

        return display?.Name ?? status.ToString();
    }
}
