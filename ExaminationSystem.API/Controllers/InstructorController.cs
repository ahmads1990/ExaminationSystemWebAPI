using ExaminationSystem.API.Common;
using ExaminationSystem.API.Extensions;
using ExaminationSystem.API.Models.Responses;
using ExaminationSystem.Application.DTOs.Courses;
using ExaminationSystem.Application.DTOs.Exams;
using ExaminationSystem.Application.DTOs.StudentExams;
using ExaminationSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExaminationSystem.API.Controllers;

/// <summary>
/// Controller for instructor-specific operations such as viewing dashboards and submissions.
/// </summary>
[ApiController]
[Route("api/[controller]")]
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
    /// Retrieves statistics for all courses assigned to the current instructor.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of course statistics detailing enrollments and exam counts.</returns>
    [HttpGet("courses")]
    public async Task<ApiResponse<List<CourseStatsDto>>> GetInstructorCourses(CancellationToken cancellationToken = default)
    {
        var stats = await _courseService.GetInstructorCoursesStats(CurrentUserId!.Value, cancellationToken);
        return new SuccessResponse<List<CourseStatsDto>>(stats);
    }

    /// <summary>
    /// Retrieves all student submissions for a specific exam owned by the current instructor.
    /// </summary>
    /// <param name="examId">The unique identifier of the exam.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of exam attempt summaries mapping to student submissions.</returns>
    [HttpGet("exams/{examId}/submissions")]
    public async Task<ApiResponse<List<AttemptSummaryDto>>> GetExamSubmissions(int examId, CancellationToken cancellationToken = default)
    {
        var (result, submissions) = await _examService.GetExamSubmissions(examId, CurrentUserId!.Value, cancellationToken);

        return result == ExamOperationResult.Success
            ? new SuccessResponse<List<AttemptSummaryDto>>(submissions!)
            : new ErrorResponse<List<AttemptSummaryDto>>(result.ToApiErrorCode());
    }

    #endregion
}
