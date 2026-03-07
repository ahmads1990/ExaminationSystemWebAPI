using ExaminationSystem.API.Authorization;
using ExaminationSystem.API.Common;
using ExaminationSystem.API.Extensions;
using ExaminationSystem.API.Models.Requests.StudentExams;
using ExaminationSystem.API.Models.Responses;
using ExaminationSystem.Application.Common;
using ExaminationSystem.Application.DTOs.StudentExams;
using ExaminationSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExaminationSystem.API.Controllers;

/// <summary>
/// Controller for managing student exam attempts and tracking dashboard metrics.
/// </summary>
public class StudentExamsController : BaseController
{
    #region Fields

    private readonly IStudentExamService _studentExamService;

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="StudentExamsController"/> class.
    /// </summary>
    /// <param name="studentExamService">The student exam service.</param>
    public StudentExamsController(IStudentExamService studentExamService)
    {
        _studentExamService = studentExamService;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Starts a new exam attempt for the current student.
    /// Returns an exam-scoped access token on success.
    /// </summary>
    /// <param name="request">The start exam attempt request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A success response with the exam access token, or an error response.</returns>
    [Authorize(Roles = Constants.StudentRoleName)]
    [HttpPost("start")]
    public async Task<ApiResponse<string>> StartExamAttempt([FromBody] StartExamAttemptRequest request, CancellationToken cancellationToken = default)
    {
        var startExamDto = request.Adapt<StartExamAttemptDto>();
        startExamDto.StudentId = CurrentUserId!.Value;

        var (result, accessToken) = await _studentExamService.StartExamAttempt(startExamDto, cancellationToken);
        return result == StudentExamAttemptResult.Success
            ? new SuccessResponse<string>(accessToken)
            : new ErrorResponse<string>(result.ToApiErrorCode());
    }

    /// <summary>
    /// Gets the exam questions for the current attempt (no IsCorrect).
    /// Requires the exam-scoped JWT (exam:answer scope).
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A success response with the list of questions, or an error response.</returns>
    [Authorize(Policy = PolicyNames.ExamAnswer)]
    [HttpGet("questions")]
    public async Task<ApiResponse<List<ExamQuestionDto>>> GetExamQuestions(CancellationToken cancellationToken = default)
    {
        var attemptId = GetExamAttemptId();
        if (attemptId is null)
            return new ErrorResponse<List<ExamQuestionDto>>(ApiErrorCode.InvalidToken);

        var (result, questions) = await _studentExamService.GetExamQuestions(attemptId.Value, CurrentUserId!.Value, cancellationToken);
        return result == StudentExamAttemptResult.Success
            ? new SuccessResponse<List<ExamQuestionDto>>(questions!)
            : new ErrorResponse<List<ExamQuestionDto>>(result.ToApiErrorCode());
    }

    /// <summary>
    /// Submits a single answer for the current exam attempt.
    /// Requires the exam-scoped JWT (exam:answer scope).
    /// </summary>
    /// <param name="answer">The answer to submit.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A success response if the answer was submitted, otherwise an error response.</returns>
    [Authorize(Policy = PolicyNames.ExamAnswer)]
    [HttpPost("answer")]
    public async Task<ApiResponse<string>> SubmitAnswer([FromBody] SubmitAnswerDto answer, CancellationToken cancellationToken = default)
    {
        var attemptId = GetExamAttemptId();
        if (attemptId is null)
            return new ErrorResponse<string>(ApiErrorCode.InvalidToken);

        var result = await _studentExamService.SubmitAnswer(answer, attemptId.Value, CurrentUserId!.Value, cancellationToken);
        return result == StudentExamAttemptResult.Success
            ? new SuccessResponse<string>(string.Empty)
            : new ErrorResponse<string>(result.ToApiErrorCode());
    }

    /// <summary>
    /// Submits multiple answers for the current exam attempt.
    /// Requires the exam-scoped JWT (exam:answer scope).
    /// </summary>
    /// <param name="answers">The list of answers to submit.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A success response if all answers were submitted, otherwise an error response.</returns>
    [Authorize(Policy = PolicyNames.ExamAnswer)]
    [HttpPost("answers")]
    public async Task<ApiResponse<string>> SubmitAnswers([FromBody] List<SubmitAnswerDto> answers, CancellationToken cancellationToken = default)
    {
        var attemptId = GetExamAttemptId();
        if (attemptId is null)
            return new ErrorResponse<string>(ApiErrorCode.InvalidToken);

        var result = await _studentExamService.SubmitAnswers(answers, attemptId.Value, CurrentUserId!.Value, cancellationToken);
        return result == StudentExamAttemptResult.Success
            ? new SuccessResponse<string>(string.Empty)
            : new ErrorResponse<string>(result.ToApiErrorCode());
    }

    /// <summary>
    /// Closes the current exam attempt, marking it as completed.
    /// Requires the exam-scoped JWT (exam:answer scope).
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A success response if the attempt was closed, otherwise an error response.</returns>
    [Authorize(Policy = PolicyNames.ExamAnswer)]
    [HttpPost("submit-attempt")]
    public async Task<ApiResponse<string>> SubmitAttempt(CancellationToken cancellationToken = default)
    {
        var attemptId = GetExamAttemptId();
        if (attemptId is null)
            return new ErrorResponse<string>(ApiErrorCode.InvalidToken);

        var result = await _studentExamService.SubmitAttempt(attemptId.Value, CurrentUserId!.Value, cancellationToken);
        return result == StudentExamAttemptResult.Success
            ? new SuccessResponse<string>(string.Empty)
            : new ErrorResponse<string>(result.ToApiErrorCode());
    }

    /// <summary>
    /// Gets the result of an exam attempt. Grades it synchronously if <= 10 questions and not graded.
    /// Requires the standard user JWT (not exam-scoped).
    /// </summary>
    /// <param name="attemptId">The optional exam attempt identifier; if null, gets the latest.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A success response with the attempt result, or an error response.</returns>
    [Authorize(Roles = Constants.StudentRoleName)]
    [HttpGet("result")]
    public async Task<ApiResponse<AttemptResultDto>> GetAttemptResult([FromQuery] int? attemptId, CancellationToken cancellationToken = default)
    {
        var (result, attemptResult) = await _studentExamService.GetAttemptResult(attemptId, CurrentUserId!.Value, cancellationToken);
        return result == StudentExamAttemptResult.Success
            ? new SuccessResponse<AttemptResultDto>(attemptResult!)
            : new ErrorResponse<AttemptResultDto>(result.ToApiErrorCode());
    }

    /// <summary>
    /// Lists all published and active exams available for the student across enrolled courses.
    /// Excludes exams that have exhausted MaxAttempts.
    /// Requires standard user JWT.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A success response populated with AvailableExamDto entries.</returns>
    [Authorize(Roles = Constants.StudentRoleName)]
    [HttpGet("available")]
    public async Task<ApiResponse<List<AvailableExamDto>>> GetAvailableExams(CancellationToken cancellationToken = default)
    {
        var availableExams = await _studentExamService.GetAvailableExams(CurrentUserId!.Value, cancellationToken);
        return new SuccessResponse<List<AvailableExamDto>>(availableExams);
    }

    /// <summary>
    /// Lists all historical exam attempts executed by the student (finished, timed out, grading, graded).
    /// Requires the standard user JWT (not exam-scoped).
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A success response with the list of previous attempts.</returns>
    [Authorize(Roles = Constants.StudentRoleName)]
    [HttpGet("history")]
    public async Task<ApiResponse<List<AttemptSummaryDto>>> GetExamHistory(CancellationToken cancellationToken = default)
    {
        var attempts = await _studentExamService.GetExamHistory(CurrentUserId!.Value, cancellationToken);
        return new SuccessResponse<List<AttemptSummaryDto>>(attempts);
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Extracts the ExamAttemptId from the current user's JWT claims.
    /// </summary>
    /// <returns>The exam attempt identifier if found, otherwise null.</returns>
    private int? GetExamAttemptId()
    {
        var claimValue = User.FindFirst(CustomClaimTypes.ExamAttemptId)?.Value;
        return int.TryParse(claimValue, out var id) ? id : null;
    }

    #endregion
}
