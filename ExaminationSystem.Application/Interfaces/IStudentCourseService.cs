using ExaminationSystem.Application.DTOs.StudentCourses;

namespace ExaminationSystem.Application.Interfaces;

public interface IStudentCourseService
{
    /// <summary>
    /// Retrieves a paginated list of course enrollments for a specific student.
    /// </summary>
    /// <param name="listDto">
    /// Filtering, sorting, and pagination options.
    /// <list type="bullet">
    /// <item><description><c>CourseTitle</c> – filter by course title.</description></item>
    /// <item><description><c>OrderBy</c> – field name used for sorting.</description></item>
    /// <item><description><c>SortDirection</c> – ascending or descending.</description></item>
    /// <item><description><c>PageIndex</c> – zero-based page index.</description></item>
    /// <item><description><c>PageSize</c> – number of items per page.</description></item>
    /// </list>
    /// </param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// A tuple containing:
    /// <list type="bullet">
    /// <item><description>The paginated enrollment data.</description></item>
    /// <item><description>Total number of matching records.</description></item>
    /// </list>
    /// </returns>
    Task<(IEnumerable<StudentEnrollmentDto> Data, int TotalCount)> ListStudentEnrollments(ListStudentEnrollmentsDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Enrolls a student in a course if all enrollment rules are satisfied.
    /// </summary>
    /// <param name="dto">Enrollment data including student and course identifiers.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// An operation result indicating success or the reason for failure:
    /// <list type="bullet">
    /// <item><description><see cref="StudentCourseOperationResult.Success"/></description></item>
    /// <item><description><see cref="StudentCourseOperationResult.CourseNotFound"/></description></item>
    /// <item><description><see cref="StudentCourseOperationResult.AlreadyEnrolled"/></description></item>
    /// <item><description><see cref="StudentCourseOperationResult.MaxEnrollmentsExceeded"/></description></item>
    /// </list>
    /// </returns>
    Task<StudentCourseOperationResult> EnrollInCourse(StudentEnrollInCourseDto dto, CancellationToken cancellationToken = default);
}
