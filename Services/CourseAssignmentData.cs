using StudyTracker.Models;

namespace StudyTracker.Services;

public sealed record CourseAssignmentData(
    Course Course,
    IReadOnlyList<User> Students,
    IReadOnlyList<string> AssignedStudentIds);
