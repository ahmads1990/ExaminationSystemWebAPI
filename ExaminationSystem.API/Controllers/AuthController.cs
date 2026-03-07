using ExaminationSystem.API.Extensions;
using ExaminationSystem.API.Models.Requests.Auth;
using ExaminationSystem.API.Models.Responses;
using ExaminationSystem.Application.DTOs.Auth;
using ExaminationSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ExaminationSystem.API.Controllers;

/// <summary>
/// Controller for authentication and user registration operations.
/// </summary>
public class AuthController : BaseController
{
    #region Fields

    private readonly IAuthService _authService;

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthController"/> class with the specified authentication service.
    /// </summary>
    /// <param name="authService">The authentication service.</param>
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Registers a new instructor to the system. A verification email will be sent.
    /// </summary>
    /// <param name="request">The instructor registration details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success response if registration completed, otherwise failure with error details.</returns>
    [HttpPost]
    public async Task<ApiResponse<int>> RegisterInstructor(RegisterInstructorRequest request, CancellationToken cancellationToken = default)
    {
        var registerInstructorDto = request.Adapt<RegisterInstructorDto>();
        var (result, id) = await _authService.RegisterInstructorAsync(registerInstructorDto, cancellationToken);

        return result == UserOperationResult.Success
            ? new SuccessResponse<int>(id, "Registration successful. Please check your email for verification.")
            : new ErrorResponse<int>(result.ToApiErrorCode());
    }

    /// <summary>
    /// Registers a new student to the system. A verification email will be sent.
    /// </summary>
    /// <param name="request">The student registration details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success response if registration completed, otherwise failure with error details.</returns>
    [HttpPost]
    public async Task<ApiResponse<int>> RegisterStudent(RegisterStudentRequest request, CancellationToken cancellationToken = default)
    {
        var registerStudentDto = request.Adapt<RegisterStudentDto>();
        var (result, id) = await _authService.RegisterStudentAsync(registerStudentDto, cancellationToken);

        return result == UserOperationResult.Success
            ? new SuccessResponse<int>(id, "Registration successful. Please check your email for verification.")
            : new ErrorResponse<int>(result.ToApiErrorCode());
    }

    /// <summary>
    /// Authenticates a user and returns a JWT token if successful.
    /// </summary>
    /// <param name="request">The login credentials.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>JWT token if login successful, otherwise failure with error details.</returns>
    [HttpPost]
    public async Task<ApiResponse<UserTokensDto>> Login(UserLoginRequest request, CancellationToken cancellationToken = default)
    {
        var loginDto = request.Adapt<UserLoginDto>();
        var (loginResult, tokens) = await _authService.LoginAsync(loginDto, cancellationToken);

        return loginResult == UserOperationResult.Success
            ? new SuccessResponse<UserTokensDto>(tokens!)
            : new ErrorResponse<UserTokensDto>(loginResult.ToApiErrorCode());
    }

    /// <summary>
    /// Verifies a user's email address using the provided confirmation token.
    /// </summary>
    /// <param name="userId">The user's unique identifier.</param>
    /// <param name="token">The email confirmation token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success response if email verified, otherwise failure with error details.</returns>
    [HttpPost]
    public async Task<ApiResponse<string>> VerifyEmail([FromQuery] int userId, [FromQuery] string token, CancellationToken cancellationToken = default)
    {
        var verificationResult = await _authService.VerifyEmailAsync(userId, token, cancellationToken);

        return verificationResult == UserEmailVerificationResult.Success
            ? new SuccessResponse<string>("")
            : new ErrorResponse<string>(verificationResult.ToApiErrorCode());
    }

    /// <summary>
    /// Resends the email verification token to the user.
    /// </summary>
    /// <param name="userId">The user's unique identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success response if token refresh initiated, otherwise failure with error details.</returns>
    [HttpPost]
    public async Task<ApiResponse<string>> ResendVerificationEmail([FromQuery] int userId, CancellationToken cancellationToken = default)
    {
        var result = await _authService.RefreshUserEmailVerificationToken(userId, cancellationToken);
        return result == UserEmailVerificationResult.EmailJobSent
            ? new SuccessResponse<string>("", "Verification email sent")
            : new ErrorResponse<string>(result.ToApiErrorCode());
    }

    /// <summary>
    /// Refreshes an expired JWT token using a valid refresh token.
    /// </summary>
    /// <param name="request">The refresh token request containing user ID and refresh token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A new JWT and refresh token pair if successful, otherwise failure with error details.</returns>
    [HttpPost]
    public async Task<ApiResponse<UserTokensDto>> RefreshToken(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        var (result, tokens) = await _authService.RefreshUserToken(request.UserId, request.RefreshToken, cancellationToken);
        return result == UserOperationResult.Success
            ? new SuccessResponse<UserTokensDto>(tokens!)
            : new ErrorResponse<UserTokensDto>(result.ToApiErrorCode());
    }

    /// <summary>
    /// Logs out the current user, blacklisting the JWT access token and revoking the refresh token.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success response if logged out successfully.</returns>
    [HttpPost("logout")]
    [Authorize]
    public async Task<ApiResponse<string>> Logout(CancellationToken cancellationToken = default)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdString, out int userId))
            return new ErrorResponse<string>(ApiErrorCode.Unauthorized);

        var jti = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
        if (string.IsNullOrEmpty(jti))
            return new ErrorResponse<string>(ApiErrorCode.InvalidToken);

        var expString = User.FindFirst(JwtRegisteredClaimNames.Exp)?.Value;
        if (!long.TryParse(expString, out long expSeconds))
            return new ErrorResponse<string>(ApiErrorCode.InvalidToken);

        var expirationDate = DateTimeOffset.FromUnixTimeSeconds(expSeconds).UtcDateTime;

        var result = await _authService.LogoutAsync(userId, jti, expirationDate, cancellationToken);
        return result == UserOperationResult.Success
            ? new SuccessResponse<string>("", "Logged out successfully")
            : new ErrorResponse<string>(result.ToApiErrorCode());
    }

    /// <summary>
    /// Initiates a password reset process by emailing an OTP to the user.
    /// </summary>
    /// <param name="request">The forgot password request detailing the user's email.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A generic success message mapping out to the user to check their email.</returns>
    [HttpPost("forgot-password")]
    public async Task<ApiResponse<string>> ForgotPassword(ForgotPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _authService.ForgotPasswordAsync(request.Email, cancellationToken);
        return result == UserOperationResult.Success
            ? new SuccessResponse<string>("", "If that email exists in our system, you will receive a password reset OTP shortly.")
            : new ErrorResponse<string>(result.ToApiErrorCode());
    }

    /// <summary>
    /// Completes the password reset process by verifying the OTP.
    /// </summary>
    /// <param name="request">The reset password payload including email, OTP, and new password.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A success message indicating password mutation.</returns>
    [HttpPost("reset-password")]
    public async Task<ApiResponse<string>> ResetPassword(ResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _authService.ResetPasswordAsync(request.Email, request.OTP, request.NewPassword, cancellationToken);
        return result == UserOperationResult.Success
            ? new SuccessResponse<string>("", "Your password has been reset successfully.")
            : new ErrorResponse<string>(result.ToApiErrorCode());
    }

    #endregion
}