using ExaminationSystemWebAPI.Models.Users;
using ExaminationSystemWebAPI.Services.AuthService;
using ExaminationSystemWebAPI.Services.InstructorService;
using ExaminationSystemWebAPI.Services.StudentService;
using ExaminationSystemWebAPI.ViewModels.Auth;
using ExaminationSystemWebAPI.ViewModels.Auth.Register;
using Mapster;
using Microsoft.AspNetCore.Mvc;

namespace ExaminationSystemWebAPI.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IInstructorService _instructorService;
    private readonly IStudentService _studentService;

    public AuthController(IAuthService authService, IInstructorService instructorService, IStudentService studentService)
    {
        _authService = authService;
        _instructorService = instructorService;
        _studentService = studentService;
    }

    [HttpPost]
    public async Task<IActionResult> RegisterStudent(RegisterStudentViewModel studentViewModel)
    {
        var authResult = await _authService.RegisterUser(studentViewModel);

        if (!authResult.IsAuthenticated)
            return BadRequest(authResult.Message);

        var studentModel = studentViewModel.Adapt<Student>();
        studentModel.AppUserID = authResult.UserID;

        _studentService.AddStudent(studentModel);

        return Ok(authResult);
    }

    [HttpPost]
    public async Task<IActionResult> RegisterInstructor(RegisterInstructorViewModel instructorViewModel)
    {
        var authResult = await _authService.RegisterUser(instructorViewModel);

        if (!authResult.IsAuthenticated)
            return BadRequest(authResult.Message);

        var instructorModel = instructorViewModel.Adapt<Instructor>();
        instructorModel.AppUserID = authResult.UserID;

        _instructorService.AddInstructor(instructorModel);

        return Ok(authResult);
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel loginViewModel)
    {
        var result = await _authService.LoginUser(loginViewModel);

        if (!result.IsAuthenticated)
            return BadRequest(result.Message);

        //Todo send confirmation mail
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> AddClaim(AddClaimViewModel claimViewModel)
    {
        var result = await _authService.AddClaim(claimViewModel);

        if (!string.IsNullOrEmpty(result))
            return BadRequest(result);

        return Ok(claimViewModel);
    }
}
