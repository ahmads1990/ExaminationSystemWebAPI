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

    [HttpPost]
    public async Task<BaseResponse<string>> RegisterInstructor(RegisterInstructorRequest request)
    {
        var registerInstructorDto = request.Adapt<RegisterInstructorDto>();
        var (registerResult, token) = await _authService.RegisterInstructor(registerInstructorDto);

        return registerResult == RegisterResult.Success ?
              new SuccessResponse<string>(token) :
              new FailureResponse<string>(ErrorCode.ValidationError, registerResult.ToString());
    }

    [HttpPost]
    public void RegisterStudent()
    {
        return;
    }

    [HttpPost]
    public void Login()
    {
        return;
    }
}
