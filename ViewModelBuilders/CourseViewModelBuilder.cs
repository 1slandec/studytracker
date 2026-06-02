using StudyTracker.Models;
using StudyTracker.ViewModels;

namespace StudyTracker.ViewModelBuilders;

public class CourseViewModelBuilder
{
    public CourseListViewModel BuildList(IEnumerable<Course> courses, bool canManageCourses)
    {
        return new CourseListViewModel
        {
            CanManageCourses = canManageCourses,
            Courses = courses
                .Select(course => BuildCourse(course, true))
                .ToList()
        };
    }

    public CourseDetailsViewModel BuildDetails(
        Course course,
        IEnumerable<StudyTaskListItemViewModel> tasks,
        bool canManageCourse,
        bool canExportTasks)
    {
        return new CourseDetailsViewModel
        {
            Course = BuildCourse(course, true),
            Tasks = tasks.ToList(),
            CanManageCourse = canManageCourse,
            CanExportTasks = canExportTasks
        };
    }

    public CourseFormViewModel BuildForm(Course course)
    {
        return new CourseFormViewModel
        {
            Id = course.Id,
            Name = course.Name,
            Description = course.Description,
            ProfessorName = course.ProfessorName
        };
    }

    public AssignCourseViewModel BuildAssignment(Course course, IEnumerable<User> students, IEnumerable<string> assignedStudentIds)
    {
        var assigned = assignedStudentIds.ToHashSet();

        return new AssignCourseViewModel
        {
            CourseId = course.Id,
            CourseName = course.Name,
            SelectedStudentIds = assigned.ToArray(),
            Students = students.Select(student => new StudentAssignmentViewModel
            {
                StudentId = student.Id,
                FullName = student.FullName,
                Email = student.Email ?? string.Empty,
                IsAssigned = assigned.Contains(student.Id)
            }).ToList()
        };
    }

    private static CourseViewModel BuildCourse(Course course, bool isAssigned)
    {
        return new CourseViewModel
        {
            Id = course.Id,
            Name = course.Name,
            Description = course.Description,
            ProfessorName = course.ProfessorName,
            TaskCount = course.Tasks.Count,
            IsAssignedToCurrentStudent = isAssigned
        };
    }
}
