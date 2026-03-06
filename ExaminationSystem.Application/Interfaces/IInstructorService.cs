using ExaminationSystem.Application.DTOs.Instructor;

namespace ExaminationSystem.Application.Interfaces;

/// <summary>
/// Service for managing instructor-related operations.
/// </summary>
public interface IInstructorService
{
    /// <summary>
    /// Adds a new instructor asynchronously.
    /// </summary>
    /// <param name="userDto">The instructor details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The operation result.</returns>
    Task<UserOperationResult> AddAsync(AddInstructorDto userDto, CancellationToken cancellationToken = default);
}

