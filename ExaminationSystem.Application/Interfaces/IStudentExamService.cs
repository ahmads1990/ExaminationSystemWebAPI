using ExaminationSystem.Application.DTOs.StudentExams;

namespace ExaminationSystem.Application.Interfaces;

/// <summary>
/// Provides operations for managing student exam attempts.
/// </summary>
public interface IStudentExamService
{
    /// <summary>
    /// Starts a new exam attempt for a student, returning an exam-scoped access token on success.
    /// </summary>
    Task<(StudentExamAttemptResult Result, string AccessToken)> StartExamAttempt(StartExamAttemptDto startExamDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the questions and choices for the given exam attempt (no IsCorrect).
    /// </summary>
    Task<(StudentExamAttemptResult Result, List<ExamQuestionDto>? Questions)> GetExamQuestions(int examAttemptId, int studentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves a single answer for an exam attempt (no grading).
    /// </summary>
    Task<StudentExamAttemptResult> SubmitAnswer(SubmitAnswerDto answerDto, int examAttemptId, int studentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves multiple answers for an exam attempt (no grading).
    /// </summary>
    Task<StudentExamAttemptResult> SubmitAnswers(List<SubmitAnswerDto> answers, int examAttemptId, int studentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Closes the exam attempt, setting its status to Completed and recording the end time.
    /// </summary>
    Task<StudentExamAttemptResult> SubmitAttempt(int examAttemptId, int studentId, CancellationToken cancellationToken = default);
}
