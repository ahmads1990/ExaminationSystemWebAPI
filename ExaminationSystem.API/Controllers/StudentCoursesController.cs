using ExaminationSystem.API.Common;
using ExaminationSystem.API.Extensions;
using ExaminationSystem.API.Models.Requests.StudentCourses;
using ExaminationSystem.API.Models.Responses;
using ExaminationSystem.Application.DTOs.StudentCourses;
using ExaminationSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExaminationSystem.API.Controllers;

/// <summary>
/// Manages course enrollments for students and instructors.
/// </summary>
/// <remarks>
/// - Students can view and manage their own enrollments.
/// - Instructors can view enrollments for any student.
/// </remarks>
[ApiController]
[Route("api/[controller]")]
public class StudentCoursesController : BaseController
{
    #region Fields

    private readonly IStudentCourseService _studentCourseService;

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="StudentCoursesController"/> class.
    /// </summary>
    /// <param name="studentCourseService">The student course service.</param>
    public StudentCoursesController(IStudentCourseService studentCourseService)
    {
        _studentCourseService = studentCourseService;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Returns the authenticated student's enrolled courses.
    /// </summary>
    /// <remarks>
    /// This endpoint returns a paginated list of courses that the current student
    /// is enrolled in.
    ///
    /// <para><b>Available sorting fields:</b></para>
    /// <list type="bullet">
    /// <item><description><c>EnrollmentDate</c></description></item>
    /// <item><description><c>Finished</c></description></item>
    /// <item><description><c>CreatedDate</c> (default)</description></item>
    /// </list>
    /// </remarks>
    /// <param name="request">The listing and filtering request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated list of the student's own enrollments.</returns>
    /// <response code="200">The paginated list of enrolled courses.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">User is not a student.</response>
    [Authorize(Roles = Constants.StudentRoleName)]
    [HttpGet("me/enrollments")]
    public async Task<PaginatedResponse<StudentEnrollmentDto>> ListMyEnrollments([FromQuery] ListStudentEnrollmentsRequest request, CancellationToken cancellationToken = default)
    {
        var listDto = request.Adapt<ListStudentEnrollmentsDto>();
        listDto.StudentId = CurrentUserId!.Value;

        var (courses, totalCount) =
            await _studentCourseService.ListStudentEnrollments(listDto, cancellationToken);

        return new PaginatedResponse<StudentEnrollmentDto>(courses, totalCount);
    }

    /// <summary>
    /// Returns the enrolled courses for a specific student.
    /// </summary>
    /// <remarks>
    /// This endpoint is intended for instructors and administrative use.
    ///
    /// <para><b>Available sorting fields:</b></para>
    /// <list type="bullet">
    /// <item><description><c>EnrollmentDate</c></description></item>
    /// <item><description><c>Finished</c></description></item>
    /// <item><description><c>CreatedDate</c> (default)</description></item>
    /// </list>
    /// </remarks>
    /// <param name="studentId">The identifier of the student.</param>
    /// <param name="request">The listing and filtering request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated list of the specified student's enrollments.</returns>
    /// <response code="200">The paginated list of the student's enrollments.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">User is not an instructor.</response>
    [Authorize(Roles = Constants.InstructorRoleName)]
    [HttpGet("{studentId:int}/enrollments")]
    public async Task<PaginatedResponse<StudentEnrollmentDto>> ListStudentEnrollments(int studentId, [FromQuery] ListStudentEnrollmentsRequest request, CancellationToken cancellationToken = default)
    {
        var listDto = request.Adapt<ListStudentEnrollmentsDto>();
        listDto.StudentId = studentId;

        var (courses, totalCount) = await _studentCourseService.ListStudentEnrollments(listDto, cancellationToken);

        return new PaginatedResponse<StudentEnrollmentDto>(courses, totalCount);
    }

    /// <summary>
    /// Enrolls the authenticated student in a course.
    /// </summary>
    /// <remarks>
    /// Enrollment may fail if:
    /// <list type="bullet">
    /// <item><description>The course does not exist.</description></item>
    /// <item><description>The student is already enrolled.</description></item>
    /// <item><description>The maximum allowed enrollments is exceeded.</description></item>
    /// </list>
    /// </remarks>
    /// <param name="request">The enrollment request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A success response if enrollment succeeded, otherwise an error response.</returns>
    /// <response code="200">Enrollment succeeded.</response>
    /// <response code="400">Enrollment failed due to a business rule.</response>
    /// <response code="401">User is not authenticated.</response>
    [Authorize(Roles = Constants.StudentRoleName)]
    [HttpPost("enroll")]
    public async Task<ApiResponse<string>> EnrollInCourse(StudentEnrollInCourseRequest request, CancellationToken cancellationToken = default)
    {
        var enrollInCourseDto = request.Adapt<StudentEnrollInCourseDto>();
        enrollInCourseDto.StudentId = CurrentUserId!.Value;

        var result = await _studentCourseService.EnrollInCourse(enrollInCourseDto, cancellationToken);

        return result == StudentCourseOperationResult.Success
            ? new SuccessResponse<string>("")
            : new ErrorResponse<string>(result.ToApiErrorCode());
    }

    #endregion
}
