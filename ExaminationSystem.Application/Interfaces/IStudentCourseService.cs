using ExaminationSystem.Application.DTOs.StudentCourses;

namespace ExaminationSystem.Application.Interfaces;

/// <summary>
/// Service for managing student course enrollments.
/// </summary>
public interface IStudentCourseService
{
    /// <summary>
    /// Retrieves a paginated list of course enrollments for a specific student.
    /// </summary>
    /// <param name="dto">Filtering, sorting, and pagination options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A tuple containing the enrollment data and the total number of matching records.</returns>
    Task<(IEnumerable<StudentEnrollmentDto> Data, int TotalCount)> ListStudentEnrollments(ListStudentEnrollmentsDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Enrolls a student in a course.
    /// </summary>
    /// <param name="dto">Enrollment data including student and course identifiers.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An operation result indicating success or failure.</returns>
    Task<StudentCourseOperationResult> EnrollInCourse(StudentEnrollInCourseDto dto, CancellationToken cancellationToken = default);
}
