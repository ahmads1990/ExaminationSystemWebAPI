namespace ExaminationSystem.Application.Interfaces;

/// <summary>
/// Defines the job for grading an exam attempt asynchronously via Hangfire.
/// </summary>
public interface IGradeExamAttemptJob
{
    /// <summary>
    /// Trigger the grading of an exam attempt.
    /// </summary>
    /// <param name="attemptId">The exam attempt ID.</param>
    /// <param name="tenantId">Optional tenant ID for job identification and debugging.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task GradeAttemptAsync(int attemptId, int? tenantId = null, CancellationToken cancellationToken = default);
}
