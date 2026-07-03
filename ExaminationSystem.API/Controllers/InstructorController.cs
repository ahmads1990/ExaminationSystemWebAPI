using ExaminationSystem.API.Common;
using ExaminationSystem.API.Extensions;
using ExaminationSystem.API.Models.Requests.Instructor;
using ExaminationSystem.API.Models.Responses;
using ExaminationSystem.Application.DTOs.Courses;
using ExaminationSystem.Application.DTOs.Exams;
using ExaminationSystem.Application.DTOs.Instructor;
using ExaminationSystem.Application.DTOs.StudentExams;
using ExaminationSystem.Application.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ExaminationSystem.API.Controllers;

/// <summary>
/// Controller for instructor-specific operations such as viewing dashboards and submissions.
/// </summary>
[Authorize(Roles = Constants.InstructorRoleName)]
public class InstructorController : BaseController
{
    #region Fields

    private readonly ICourseService _courseService;
    private readonly IExamService _examService;

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="InstructorController"/> class.
    /// </summary>
    /// <param name="courseService">The course service.</param>
    /// <param name="examService">The exam service.</param>
    public InstructorController(ICourseService courseService, IExamService examService)
    {
        _courseService = courseService;
        _examService = examService;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Retrieves a paginated and filtered list of statistics for all courses assigned to the current instructor.
    /// </summary>
    /// <param name="request">The listing, filtering, and pagination request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated list of course statistics.</returns>
    [HttpGet("courses")]
    [ProducesResponseType(typeof(SuccessResponse<PaginatedResponse<CourseStatsDto>>), StatusCodes.Status200OK)]
    public async Task<ApiResponse<PaginatedResponse<CourseStatsDto>>> GetInstructorCourses([FromQuery] ListInstructorCoursesRequest request, CancellationToken cancellationToken = default)
    {
        var listDto = request.Adapt<ListInstructorCoursesDto>();
        var (stats, totalCount) = await _courseService.GetInstructorCoursesStats(CurrentUserId!.Value, listDto, cancellationToken);
        return new SuccessResponse<PaginatedResponse<CourseStatsDto>>(new PaginatedResponse<CourseStatsDto>(stats, totalCount));
    }

    /// <summary>
    /// Retrieves a paginated and filtered list of student submissions for a specific exam owned by the current instructor.
    /// </summary>
    /// <param name="examId">The unique identifier of the exam.</param>
    /// <param name="request">The listing, filtering, and pagination request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated list of exam attempt summaries mapping to student submissions.</returns>
    [HttpGet("exams/{examId}/submissions")]
    [ProducesResponseType(typeof(SuccessResponse<PaginatedResponse<AttemptSummaryDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse<PaginatedResponse<AttemptSummaryDto>>), StatusCodes.Status400BadRequest)]
    public async Task<ApiResponse<PaginatedResponse<AttemptSummaryDto>>> GetExamSubmissions(int examId, [FromQuery] ListExamSubmissionsRequest request, CancellationToken cancellationToken = default)
    {
        var listDto = request.Adapt<ListExamSubmissionsDto>();
        var (result, submissions, totalCount) = await _examService.GetExamSubmissions(examId, CurrentUserId!.Value, listDto, cancellationToken);

        return result == ExamOperationResult.Success
            ? new SuccessResponse<PaginatedResponse<AttemptSummaryDto>>(new PaginatedResponse<AttemptSummaryDto>(submissions!, totalCount))
            : new ErrorResponse<PaginatedResponse<AttemptSummaryDto>>(result.ToApiErrorCode());
    }

    #endregion
}
