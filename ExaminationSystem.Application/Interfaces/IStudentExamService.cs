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

    /// <summary>
    /// Gets the result of an exam attempt. Grades it synchronously if <= 10 questions and not graded.
    /// </summary>
    /// <param name="examAttemptId">The optional exam attempt identifier; if null, gets the latest.</param>
    /// <param name="studentId">The student identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A tuple containing the operation result and the attempt result DTO if successful.</returns>
    Task<(StudentExamAttemptResult Result, AttemptResultDto? AttemptResult)> GetAttemptResult(int? examAttemptId, int studentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Grades an exam attempt, calculating the final score and updating its status to Graded.
    /// </summary>
    /// <param name="attemptId">The exam attempt identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task GradeAttemptAsync(int attemptId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Listed available (published and within deadline) exams for a student to attempt.
    /// Filters out exams the student has already run out of attempts for.
    /// </summary>
    /// <param name="studentId">The student identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of available exams details.</returns>
    Task<List<AvailableExamDto>> GetAvailableExams(int studentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists all exam attempts historical evaluations for a student (Completed, Timedout, Grading, Graded).
    /// </summary>
    /// <param name="studentId">The student identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of past attempts summaries.</returns>
    Task<List<AttemptSummaryDto>> GetExamHistory(int studentId, CancellationToken cancellationToken = default);
}
