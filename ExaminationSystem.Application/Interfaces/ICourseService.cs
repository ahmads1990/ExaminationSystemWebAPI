using ExaminationSystem.Application.DTOs.Courses;

namespace ExaminationSystem.Application.Interfaces;

public interface ICourseService
{
    /// <summary>
    /// Retrieves a paginated, sorted, and filtered list of courses.
    /// </summary>
    /// <param name="listDto"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<(IEnumerable<CourseDto> Data, int TotalCount)> GetAll(ListCoursesDto listDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new course to the system after performing validation and business rule checks.
    /// </summary>
    /// <param name="courseDto">The data transfer object containing the course details.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>
    /// A tuple containing the <see cref="CourseOperationResult"/> indicating the outcome 
    /// and the generated ID of the new course if successful; otherwise, 0.
    /// </returns>
    Task<(CourseOperationResult Result, int Id)> Add(AddCourseDto courseDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates specific informational fields of an existing course.
    /// </summary>
    /// <param name="courseDto">The data transfer object containing the updated course information and its unique identifier.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="CourseOperationResult"/> indicating whether the update was successful or if the course was not found.</returns>
    Task<CourseOperationResult> UpdateInfo(UpdateCourseDto courseDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the specified course asynchronously.
    /// </summary>
    /// <param name="courseDto">An object containing the details of the course to delete. Cannot be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the delete operation.</param>
    /// <returns>A task that represents the asynchronous delete operation. The task result contains a CourseOperationResult
    /// indicating the outcome of the operation.</returns>
    Task<CourseOperationResult> Delete(DeleteCourseDto courseDto, CancellationToken cancellationToken = default);
}
