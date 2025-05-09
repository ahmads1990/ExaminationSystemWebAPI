using ExaminationSystem.Application.DTOs.Auth;
using ExaminationSystem.Application.DTOs.Instructor;
using ExaminationSystem.Application.DTOs.Users;
using ExaminationSystem.Application.Interfaces;

namespace ExaminationSystem.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserService _userService;
    private readonly IInstructorService _instructorService;

    public AuthService(IUserService userService, IInstructorService instructorService)
    {
        _userService = userService;
        _instructorService = instructorService;
    }

    public async Task RegisterInstructor(RegisterInstructorDto registerInstructorDto)
    {
        // Input validation
        if (string.IsNullOrEmpty(registerInstructorDto.Email) ||
            string.IsNullOrEmpty(registerInstructorDto.Password))
        {
            return;
        }

        // Save user
        var addUserDto = registerInstructorDto.Adapt<AddUserDto>();
        (AddUserResult addUserResult, int userId) = await _userService.Add(addUserDto);

        // Save instructor
        var addInstructorDto = registerInstructorDto.Adapt<AddInstructorDto>();
        addInstructorDto.AppUserId = userId;
        (AddInstructorResult addInstructorResult, int instructorId) = await _instructorService.Add(addInstructorDto);
        
        // Create Token
        // Return success
    }
}

