using ExaminationSystem.Application.DTOs.Auth;
using ExaminationSystem.Application.InfraInterfaces;
using ExaminationSystem.Application.Interfaces;
using ExaminationSystem.Domain.Entities;

namespace ExaminationSystem.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserService _userService;
    private readonly IPasswordHelper _passwordHelper;

    public AuthService(IUserService userService, IPasswordHelper passwordHelper)
    {
        _userService = userService;
        _passwordHelper = passwordHelper;
    }

    public async Task RegisterInstructor(RegisterInstructorDto registerInstructorDto)
    {
        // Validation first
        // Check email is unique
        var isEmailUnique = await _userService.IsUserEmailUnique(registerInstructorDto.Email);
        if (!isEmailUnique)
            return;
        // Hash password
        var hashedPassword = _passwordHelper.HashPassword(registerInstructorDto.Password);
        // Save user
        var userDto = registerInstructorDto.Adapt<AppUser>();
        _userService.AddUser(user);
        // Return success
    }
}

