using ExaminationSystem.Application.Interfaces;

namespace ExaminationSystem.Application.UseCases;

/// <summary>
/// Implements the Hangfire job for grading exam attempts.
/// Contains no business logic, delegates to IStudentExamService.
/// </summary>
public class GradeExamAttemptJob : IGradeExamAttemptJob
{
    private readonly IStudentExamService _studentExamService;

    public GradeExamAttemptJob(IStudentExamService studentExamService)
    {
        _studentExamService = studentExamService;
    }

    /// <inheritdoc />
    public async Task GradeAttemptAsync(int attemptId, CancellationToken cancellationToken = default)
    {
        await _studentExamService.GradeAttemptAsync(attemptId, cancellationToken);
    }
}
