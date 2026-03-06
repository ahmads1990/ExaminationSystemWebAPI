using ExaminationSystem.Application.DTOs.StudentExams;

namespace ExaminationSystem.Application.Interfaces;

/// <summary>
/// Provides operations for managing student exam attempts.
/// </summary>
public interface IStudentExamService
{
    /// <summary>
    /// Starts a new exam attempt for a student.
    /// </summary>
    /// <param name="startExamDto">The exam attempt details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A tuple containing the operation result and an access token if successful.</returns>
    Task<(StudentExamAttemptResult Result, string AccessToken)> StartExamAttempt(StartExamAttemptDto startExamDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the questions and choices for an exam attempt.
    /// </summary>
    /// <param name="examAttemptId">The exam attempt identifier.</param>
    /// <param name="studentId">The student identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A tuple containing the operation result and the list of questions if successful.</returns>
    Task<(StudentExamAttemptResult Result, List<ExamQuestionDto>? Questions)> GetExamQuestions(int examAttemptId, int studentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves a single answer for an exam attempt.
    /// </summary>
    /// <param name="answerDto">The answer details.</param>
    /// <param name="examAttemptId">The exam attempt identifier.</param>
    /// <param name="studentId">The student identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The operation result.</returns>
    Task<StudentExamAttemptResult> SubmitAnswer(SubmitAnswerDto answerDto, int examAttemptId, int studentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves multiple answers for an exam attempt.
    /// </summary>
    /// <param name="answers">The list of answers.</param>
    /// <param name="examAttemptId">The exam attempt identifier.</param>
    /// <param name="studentId">The student identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The operation result.</returns>
    Task<StudentExamAttemptResult> SubmitAnswers(List<SubmitAnswerDto> answers, int examAttemptId, int studentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Submits and closes an exam attempt.
    /// </summary>
    /// <param name="examAttemptId">The exam attempt identifier.</param>
    /// <param name="studentId">The student identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The operation result.</returns>
    Task<StudentExamAttemptResult> SubmitAttempt(int examAttemptId, int studentId, CancellationToken cancellationToken = default);
}
