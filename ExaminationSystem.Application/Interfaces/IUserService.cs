using ExaminationSystem.Application.DTOs.Auth;
using ExaminationSystem.Application.DTOs.Users;

namespace ExaminationSystem.Application.Interfaces;

/// <summary>
/// Service for managing user-related operations.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Adds a new user asynchronously.
    /// </summary>
    /// <param name="userDto">The user details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A tuple containing the operation result and the new user ID.</returns>
    Task<(UserOperationResult Result, int Id)> AddAsync(AddUserDto userDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifies a user's password.
    /// </summary>
    /// <param name="userLoginDto">The login details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A tuple containing the operation result and the user ID if successful.</returns>
    Task<(UserOperationResult Result, int? Id)> VerifyUserPassword(UserLoginDto userLoginDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves basic information for a user by their identifier.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user basic information if found, otherwise null.</returns>
    Task<UserBasicInfoDto?> GetUserBasicInfoById(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Confirms a user's email address.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The email verification result.</returns>
    Task<UserEmailVerificationResult> ConfirmUserEmail(int userId, CancellationToken cancellationToken = default);
}
