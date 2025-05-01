using ExaminationSystem.API.Models.Requests.Auth;
using ExaminationSystem.Application.DTOs.Auth;
using ExaminationSystem.Application.Interfaces;

namespace ExaminationSystem.API.Controllers;

public class AuthController : BaseController
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    public void RegisterInstructor(RegisterInstructorRequest request)
    {
        var registerInstructorDto = request.Adapt<RegisterInstructorDto>();
        _authService.RegisterInstructor(registerInstructorDto);
        return;
    }

    public void RegisterStudent()
    {
        return;
    }

    public void Login()
    {
        return;
    }
}
