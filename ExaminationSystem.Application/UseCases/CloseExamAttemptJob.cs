using ExaminationSystem.Application.Interfaces;
using ExaminationSystem.Domain.Common;
using ExaminationSystem.Domain.Entities;
using ExaminationSystem.Domain.Interfaces;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ExaminationSystem.Application.UseCases;

/// <summary>
/// Hangfire job that auto-closes an exam attempt when the time expires.
/// Sets status to <see cref="ExamAttemptStatus.TimedOut"/> so it can be
/// distinguished from student-submitted (<see cref="ExamAttemptStatus.Completed"/>).
/// Enqueues grading job if the exam has more than 10 questions.
/// </summary>
public class CloseExamAttemptJob : ICloseExamAttemptJob
{
    private readonly IRepository<ExamAttempt> _examAttemptRepo;
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly ILogger<CloseExamAttemptJob> _logger;

    public CloseExamAttemptJob(IRepository<ExamAttempt> examAttemptRepo, IBackgroundJobClient backgroundJobClient, ILogger<CloseExamAttemptJob> logger)
    {
        _examAttemptRepo = examAttemptRepo;
        _backgroundJobClient = backgroundJobClient;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task ExecuteAsync(int examAttemptId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Auto-close job started for attempt {AttemptId}", examAttemptId);

        var attemptData = await _examAttemptRepo.GetByID(examAttemptId)
            .Select(a => new
            {
                Attempt = a,
                QuestionCount = a.Exam != null ? a.Exam.ExamQuestions.Count : 0
            })
            .FirstOrDefaultAsync(cancellationToken);

        var attempt = attemptData?.Attempt;

        // Idempotent — do nothing if already closed by student submission
        if (attempt is null || attempt.ExamAttemptStatus != ExamAttemptStatus.InProgress)
        {
            _logger.LogInformation("Attempt {AttemptId} already closed, skipping", examAttemptId);
            return;
        }

        attempt.ExamAttemptStatus = ExamAttemptStatus.TimedOut;
        attempt.EndTime = DateTime.UtcNow;

        _examAttemptRepo.Update(attempt);
        await _examAttemptRepo.SaveChanges(cancellationToken);

        _logger.LogInformation("Attempt {AttemptId} auto-closed (TimedOut)", attempt.ID);

        // Enqueue grading job if it has > threshold
        if (attemptData?.QuestionCount > Constants.ImmediateExamGradingThreshold)
        {
            _backgroundJobClient.Enqueue<IGradeExamAttemptJob>(
                job => job.GradeAttemptAsync(attempt.ID, CancellationToken.None));
        }
    }
}
