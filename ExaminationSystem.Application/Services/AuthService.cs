using ExaminationSystem.Application.DTOs.Auth;
using ExaminationSystem.Application.DTOs.Instructor;
using ExaminationSystem.Application.DTOs.Users;
using ExaminationSystem.Application.InfraInterfaces;
using ExaminationSystem.Application.Interfaces;

namespace ExaminationSystem.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserService _userService;
    private readonly IInstructorService _instructorService;
    private readonly ITokenHelper _tokenHelper;

    public AuthService(IUserService userService, IInstructorService instructorService, ITokenHelper tokenHelper)
    {
        _userService = userService;
        _instructorService = instructorService;
        _tokenHelper = tokenHelper;
    }

    public async Task<(RegisterResult result, string token)> RegisterInstructor(RegisterInstructorDto registerInstructorDto)
    {
        // Input validation
        if (string.IsNullOrEmpty(registerInstructorDto.Email) ||
            string.IsNullOrEmpty(registerInstructorDto.Password))
            return (RegisterResult.ValidationFailed, string.Empty);

        // Save user
        var addUserDto = registerInstructorDto.Adapt<AddUserDto>();
        var (addUserResult, userId) = await _userService.Add(addUserDto);

        if (addUserResult != AddUserResult.Success)
            return (RegisterResult.UserCreationFailed, string.Empty);

        // Save instructor
        var addInstructorDto = registerInstructorDto.Adapt<AddInstructorDto>();
        addInstructorDto.AppUserId = userId;
        var (addInstructorResult, instructorId) = await _instructorService.Add(addInstructorDto);

        if (addInstructorResult != AddInstructorResult.Success)
            return (RegisterResult.UserCreationFailed, string.Empty);

        // Create Token
        var token = _tokenHelper.GenerateToken(
            new UserTokenBaseClaims(userId, addUserDto.Username, addUserDto.Email),
            new List<UserClaim>
            {
                new("Role", "Instructor"),
                new("RoleId", instructorId.ToString())
            }
        );

        if (string.IsNullOrEmpty(token))
            return (RegisterResult.TokenGenerationFailed, string.Empty);

        // Return success
        return (RegisterResult.Success, token);
    }
}

