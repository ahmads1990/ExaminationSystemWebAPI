using ExaminationSystem.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace ExaminationSystem.Application.UseCases;

/// <summary>
/// Implements the Hangfire job for grading exam attempts.
/// Contains no business logic, delegates to IStudentExamService.
/// </summary>
public class GradeExamAttemptJob : IGradeExamAttemptJob
{
    private readonly IStudentExamService _studentExamService;
    private readonly ILogger<GradeExamAttemptJob> _logger;

    public GradeExamAttemptJob(IStudentExamService studentExamService, ILogger<GradeExamAttemptJob> logger)
    {
        _studentExamService = studentExamService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task GradeAttemptAsync(int attemptId, int? tenantId = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Grade attempt job started for AttemptId: {AttemptId} (TenantId: {TenantId})", attemptId, tenantId?.ToString() ?? "N/A");
        await _studentExamService.GradeAttemptAsync(attemptId, cancellationToken);
        _logger.LogInformation("Grade attempt job finished successfully for AttemptId: {AttemptId} (TenantId: {TenantId})", attemptId, tenantId?.ToString() ?? "N/A");
    }
}
