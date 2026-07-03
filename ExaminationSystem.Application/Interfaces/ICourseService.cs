using ExaminationSystem.Application.DTOs.Courses;
using ExaminationSystem.Application.DTOs.Instructor;

namespace ExaminationSystem.Application.Interfaces;

/// <summary>
/// Service for managing course-related operations.
/// </summary>
public interface ICourseService
{
    /// <summary>
    /// Retrieves a paginated, sorted, and filtered list of courses.
    /// </summary>
    /// <param name="listDto">The filtering and pagination parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A tuple containing the list of courses and the total count.</returns>
    Task<(IEnumerable<CourseDto> Data, int TotalCount)> GetAll(ListCoursesDto listDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new course to the system.
    /// </summary>
    /// <param name="courseDto">The course details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A tuple containing the operation result and the new course ID.</returns>
    Task<(CourseOperationResult Result, int Id)> Add(AddCourseDto courseDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing course's information.
    /// </summary>
    /// <param name="courseDto">The updated course details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The operation result.</returns>
    Task<CourseOperationResult> UpdateInfo(UpdateCourseDto courseDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a course.
    /// </summary>
    /// <param name="courseDto">The deletion details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The operation result.</returns>
    Task<CourseOperationResult> Delete(DeleteCourseDto courseDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets course statistics for an instructor dashboard with search filters and pagination.
    /// </summary>
    /// <param name="instructorId">The instructor identifier.</param>
    /// <param name="listDto">The pagination and filter DTO.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A tuple containing a list of course statistics and the total count.</returns>
    Task<(IEnumerable<CourseStatsDto> Data, int TotalCount)> GetInstructorCoursesStats(int instructorId, ListInstructorCoursesDto listDto, CancellationToken cancellationToken = default);
}
