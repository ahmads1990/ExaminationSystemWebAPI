using ExaminationSystem.Application.DTOs.Auth;

namespace ExaminationSystem.Application.Interfaces;

/// <summary>
/// Service for handling authentication, registration, and token management.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Registers a new instructor.
    /// </summary>
    /// <param name="registerInstructorDto">The registration details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A tuple containing the operation result and the new instructor ID.</returns>
    Task<(UserOperationResult Result, int Id)> RegisterInstructorAsync(RegisterInstructorDto registerInstructorDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Registers a new student.
    /// </summary>
    /// <param name="registerStudentDto">The registration details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A tuple containing the operation result and the new student ID.</returns>
    Task<(UserOperationResult Result, int Id)> RegisterStudentAsync(RegisterStudentDto registerStudentDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Authenticates a user.
    /// </summary>
    /// <param name="userLoginDto">The login credentials.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A tuple containing the operation result and the generated tokens if successful.</returns>
    Task<(UserOperationResult Result, UserTokensDto? tokensDto)> LoginAsync(UserLoginDto userLoginDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifies a user's email address.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="token">The verification token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The verification result.</returns>
    Task<UserEmailVerificationResult> VerifyEmailAsync(int userId, string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates and sends a new email verification token for a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The result of the refresh operation.</returns>
    Task<UserEmailVerificationResult> RefreshUserEmailVerificationToken(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Refreshes a user's access token using a refresh token.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="refreshToken">The refresh token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A tuple containing the operation result and the new tokens if successful.</returns>
    Task<(UserOperationResult result, UserTokensDto? tokensDto)> RefreshUserToken(int userId, string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a scoped token for an exam attempt.
    /// </summary>
    /// <param name="createTokenDto">The token creation details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A tuple containing the operation result and the generated token.</returns>
    Task<(UserOperationResult result, string token)> CreateExamAttemptToken(CreateExamTokenDto createTokenDto, CancellationToken cancellationToken = default);
}
