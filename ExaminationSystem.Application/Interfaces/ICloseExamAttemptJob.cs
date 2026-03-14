namespace ExaminationSystem.Application.Interfaces;

/// <summary>
/// Contract for the Hangfire job that auto-closes a timed-out exam attempt.
/// Defined in Application so the service layer can enqueue without a
/// direct dependency on Infrastructure.
/// </summary>
public interface ICloseExamAttemptJob
{
    /// <summary>
    /// Marks the attempt as <see cref="ExamAttemptStatus.TimedOut"/> and sets <c>EndTime</c>.
    /// Idempotent — does nothing if the attempt is already closed.
    /// </summary>
    /// <param name="examAttemptId">The exam attempt ID.</param>
    /// <param name="tenantId">Optional tenant ID for job identification and debugging.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task ExecuteAsync(int examAttemptId, int? tenantId = null, CancellationToken cancellationToken = default);
}
