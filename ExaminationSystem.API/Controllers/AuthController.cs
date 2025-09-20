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
    /// Adds a new instructor to the system and returns a JWT token if successful.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<BaseResponse<string>> RegisterInstructor(RegisterInstructorRequest request, CancellationToken cancellationToken = default)
    {
        var registerInstructorDto = request.Adapt<RegisterInstructorDto>();
        var (registerResult, token) = await _authService.RegisterInstructorAsync(registerInstructorDto, cancellationToken);

        return registerResult == UserOperationResult.Success ?
              new SuccessResponse<string>(token) :
              new FailureResponse<string>(ErrorCode.ValidationError, registerResult.ToString());
    }

    /// <summary>
    /// Adds a new student to the system and returns a JWT token if successful.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<BaseResponse<string>> RegisterStudent(RegisterStudentRequest request, CancellationToken cancellation)
    {
        var registerStudentDto = request.Adapt<RegisterStudentDto>();
        var (registerResult, token) = await _authService.RegisterStudentAsync(registerStudentDto, cancellation);

        return registerResult == UserOperationResult.Success ?
              new SuccessResponse<string>(token) :
              new FailureResponse<string>(ErrorCode.ValidationError, registerResult.ToString());
    }

    [HttpPost]
    public async Task<BaseResponse<string>> Login(UserLoginRequest request, CancellationToken cancellationToken)
    {
        var loginDto = request.Adapt<UserLoginDto>();
        var (loginResult, token) = await _authService.LoginAsync(loginDto, cancellationToken);

        return loginResult == UserOperationResult.Success ?
              new SuccessResponse<string>(token) :
              new FailureResponse<string>(ErrorCode.ValidationError, loginResult.ToString());
    }
}
