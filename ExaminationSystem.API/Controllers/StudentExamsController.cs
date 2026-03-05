using ExaminationSystem.API.Authorization;
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
/// Controller for managing student exam attempts.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class StudentExamsController : BaseController
{
    private readonly IStudentExamService _studentExamService;

    public StudentExamsController(IStudentExamService studentExamService)
    {
        _studentExamService = studentExamService;
    }

    /// <summary>
    /// Starts a new exam attempt for the current student.
    /// Returns an exam-scoped access token on success.
    /// </summary>
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

    #region Private Methods

    /// <summary>
    /// Extracts the ExamAttemptId from the current user's JWT claims.
    /// </summary>
    private int? GetExamAttemptId()
    {
        var claimValue = User.FindFirst(CustomClaimTypes.ExamAttemptId)?.Value;
        return int.TryParse(claimValue, out var id) ? id : null;
    }

    #endregion
}
