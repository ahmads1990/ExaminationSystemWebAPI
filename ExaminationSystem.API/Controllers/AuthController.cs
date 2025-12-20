using ExaminationSystem.API.Models.Requests.Auth;
using ExaminationSystem.API.Models.Responses;
using ExaminationSystem.Application.DTOs.Auth;
using ExaminationSystem.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ExaminationSystem.API.Controllers;

public class AuthController : BaseController
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Registers a new instructor to the system. A verification email will be sent.
    /// </summary>
    /// <param name="request">The instructor registration details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success response if registration completed, otherwise failure with error details.</returns>
    [HttpPost]
    public async Task<BaseResponse<int>> RegisterInstructor(RegisterInstructorRequest request, CancellationToken cancellationToken = default)
    {
        var registerInstructorDto = request.Adapt<RegisterInstructorDto>();
        var (result, id) = await _authService.RegisterInstructorAsync(registerInstructorDto, cancellationToken);

        return result == UserOperationResult.Success
            ? new SuccessResponse<int>(id, UserOperationResult.SuccessCheckMail.ToString())
            : new FailureResponse<int>(ErrorCode.Error, result.ToString());
    }

    /// <summary>
    /// Registers a new student to the system. A verification email will be sent.
    /// </summary>
    /// <param name="request">The student registration details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success response if registration completed, otherwise failure with error details.</returns>
    [HttpPost]
    public async Task<BaseResponse<int>> RegisterStudent(RegisterStudentRequest request, CancellationToken cancellationToken = default)
    {
        var registerStudentDto = request.Adapt<RegisterStudentDto>();
        var (result, id) = await _authService.RegisterStudentAsync(registerStudentDto, cancellationToken);

        return result == UserOperationResult.Success
            ? new SuccessResponse<int>(id, UserOperationResult.SuccessCheckMail.ToString())
            : new FailureResponse<int>(ErrorCode.Error, result.ToString());
    }

    /// <summary>
    /// Authenticates a user and returns a JWT token if successful.
    /// </summary>
    /// <param name="request">The login credentials.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>JWT token if login successful, otherwise failure with error details.</returns>
    [HttpPost]
    public async Task<BaseResponse<string>> Login(UserLoginRequest request, CancellationToken cancellationToken = default)
    {
        var loginDto = request.Adapt<UserLoginDto>();
        var (loginResult, token) = await _authService.LoginAsync(loginDto, cancellationToken);

        return loginResult == UserOperationResult.Success
            ? new SuccessResponse<string>(token, loginResult.ToString())
            : new FailureResponse<string>(ErrorCode.Error, loginResult.ToString());
    }

    /// <summary>
    /// Verifies a user's email address using the provided confirmation token.
    /// </summary>
    /// <param name="userId">The user's unique identifier.</param>
    /// <param name="token">The email confirmation token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success response if email verified, otherwise failure with error details.</returns>
    [HttpPost]
    public async Task<BaseResponse<string>> VerifyEmail([FromQuery] int userId, [FromQuery] string token, CancellationToken cancellationToken = default)
    {
        var verificationResult = await _authService.VerifyEmailAsync(userId, token, cancellationToken);

        return verificationResult == UserEmailVerificationResult.Success
            ? new SuccessResponse<string>("", UserEmailVerificationResult.Success.ToString())
            : new FailureResponse<string>(ErrorCode.Error, verificationResult.ToString());
    }

    /// <summary>
    /// Resends the email verification token to the user.
    /// </summary>
    /// <param name="userId">The user's unique identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success response if token refresh initiated, otherwise failure with error details.</returns>
    [HttpPost]
    public async Task<BaseResponse<string>> ResendVerificationEmail([FromQuery] int userId, CancellationToken cancellationToken = default)
    {
        var result = await _authService.RefreshUserEmailVerificationToken(userId, cancellationToken);
        return result == UserEmailVerificationResult.EmailJobSent
            ? new SuccessResponse<string>("", UserEmailVerificationResult.EmailJobSent.ToString())
            : new FailureResponse<string>(ErrorCode.Error, result.ToString());
    }
}