using ExaminationSystem.Application.DTOs.Student;

namespace ExaminationSystem.Application.Interfaces;

/// <summary>
/// Service for managing student-related operations.
/// </summary>
public interface IStudentService
{
    /// <summary>
    /// Adds a new student asynchronously.
    /// </summary>
    /// <param name="studentDto">The student details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The operation result.</returns>
    Task<UserOperationResult> AddAsync(AddStudentDto studentDto, CancellationToken cancellationToken = default);
}
