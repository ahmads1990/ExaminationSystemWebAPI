using ExaminationSystem.Application.Interfaces;
using ExaminationSystem.Domain.Entities;
using ExaminationSystem.Domain.Interfaces;

namespace ExaminationSystem.Infrastructure.Jobs;

/// <summary>
/// Hangfire job that auto-closes an exam attempt when the time expires.
/// Sets status to <see cref="ExamAttemptStatus.TimedOut"/> so it can be
/// distinguished from student-submitted (<see cref="ExamAttemptStatus.Completed"/>).
/// </summary>
public class CloseExamAttemptJob : ICloseExamAttemptJob
{
    private readonly IRepository<ExamAttempt> _examAttemptRepo;

    public CloseExamAttemptJob(IRepository<ExamAttempt> examAttemptRepo)
    {
        _examAttemptRepo = examAttemptRepo;
    }

    /// <inheritdoc/>
    public async Task ExecuteAsync(int examAttemptId, CancellationToken cancellationToken = default)
    {
        var attempt = await _examAttemptRepo.GetByID(examAttemptId, cancellationToken);

        // Idempotent — do nothing if already closed by student submission
        if (attempt is null || attempt.ExamAttemptStatus != ExamAttemptStatus.InProgress)
            return;

        attempt.ExamAttemptStatus = ExamAttemptStatus.TimedOut;
        attempt.EndTime = DateTime.UtcNow;

        _examAttemptRepo.Update(attempt);
        await _examAttemptRepo.SaveChanges(cancellationToken);
    }
}
