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
    Task ExecuteAsync(int examAttemptId, CancellationToken cancellationToken = default);
}
