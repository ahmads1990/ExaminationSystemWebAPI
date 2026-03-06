namespace ExaminationSystem.Application.Interfaces;

/// <summary>
/// Defines the job for grading an exam attempt asynchronously via Hangfire.
/// </summary>
public interface IGradeExamAttemptJob
{
    /// <summary>
    /// Trigger the grading of an exam attempt.
    /// </summary>
    Task GradeAttemptAsync(int attemptId, CancellationToken cancellationToken = default);
}
