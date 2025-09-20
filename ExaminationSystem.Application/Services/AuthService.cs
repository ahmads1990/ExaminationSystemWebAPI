using ExaminationSystem.Application.DTOs.Auth;
using ExaminationSystem.Application.DTOs.Instructor;
using ExaminationSystem.Application.DTOs.Student;
using ExaminationSystem.Application.DTOs.Users;
using ExaminationSystem.Application.InfraInterfaces;
using ExaminationSystem.Application.Interfaces;

namespace ExaminationSystem.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserService _userService;
    private readonly IInstructorService _instructorService;
    private readonly IStudentService _studentService;
    private readonly ITokenHelper _tokenHelper;
    public AuthService(IUserService userService, IInstructorService instructorService, IStudentService studentService, ITokenHelper tokenHelper)
    {
        _userService = userService;
        _instructorService = instructorService;
        _studentService = studentService;
        _tokenHelper = tokenHelper;
    }

    #region Public Methods

    /// <summary>
    /// Registers a new instructor and returns a result with a token if successful.
    /// </summary>
    /// <param name="registerInstructorDto">Instructor registration data.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>
    /// A tuple with:
    /// <list type="bullet">
    ///   <item><description>The operation result.</description></item>
    ///   <item><description>A JWT token if successful, otherwise empty.</description></item>
    /// </list>
    /// </returns>
    public async Task<(UserOperationResult Result, string Token)> RegisterInstructorAsync(RegisterInstructorDto registerInstructorDto, CancellationToken cancellationToken = default)
    {
        // Prepare user dto
        var addUserDto = registerInstructorDto.Adapt<AddUserDto>();
        addUserDto.Role = UserRole.Instructor;
        addUserDto.Code = GenerateUserCode(UserRole.Instructor);

        // Save user
        var (addUserResult, userId) = await _userService.AddAsync(addUserDto, cancellationToken);
        if (addUserResult != UserOperationResult.Success)
            return (UserOperationResult.UserCreationFailed, string.Empty);

        // Prepare instructor dto
        var addInstructorDto = registerInstructorDto.Adapt<AddInstructorDto>();
        addInstructorDto.AppUserId = userId;

        // Save instructor
        var (addInstructorResult, instructorId) = await _instructorService.AddAsync(addInstructorDto, cancellationToken);
        if (addInstructorResult != UserOperationResult.Success)
            return (UserOperationResult.UserCreationFailed, string.Empty);

        // Create Token
        var token = _tokenHelper.GenerateToken(
            new UserTokenBaseClaims(userId, addUserDto.Username, addUserDto.Email),
            new List<UserClaim>
            {
                new("RoleId", ((int)UserRole.Instructor).ToString()),
                new("Name",  addUserDto.Name)
            }
        );

        if (string.IsNullOrEmpty(token))
            return (UserOperationResult.TokenGenerationFailed, string.Empty);

        // Return success
        return (UserOperationResult.Success, token);
    }

    /// <summary>
    /// Registers a new student and returns a result with a token if successful.
    /// </summary>
    /// <param name="registerStudentDto"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<(UserOperationResult Result, string Token)> RegisterStudentAsync(RegisterStudentDto registerStudentDto, CancellationToken cancellationToken = default)
    {
        // Prepare user dto
        var addUserDto = registerStudentDto.Adapt<AddUserDto>();
        addUserDto.Role = UserRole.Student;
        addUserDto.Code = GenerateUserCode(UserRole.Student);

        // Save user
        var (addUserResult, userId) = await _userService.AddAsync(addUserDto, cancellationToken);
        if (addUserResult != UserOperationResult.Success)
            return (UserOperationResult.UserCreationFailed, string.Empty);

        // Prepare student dto
        var addStudentDto = registerStudentDto.Adapt<AddStudentDto>();
        addStudentDto.AppUserId = userId;

        // Save student
        var (addStudentResult, studentId) = await _studentService.AddAsync(addStudentDto, cancellationToken);
        if (addStudentResult != UserOperationResult.Success)
            return (UserOperationResult.UserCreationFailed, string.Empty);

        // Create Token
        var token = _tokenHelper.GenerateToken(
            new UserTokenBaseClaims(userId, addUserDto.Username, addUserDto.Email),
            new List<UserClaim>
            {
                new("RoleId", ((int)UserRole.Student).ToString()),
                new("Name",  addUserDto.Name)
            }
        );

        if (string.IsNullOrEmpty(token))
            return (UserOperationResult.TokenGenerationFailed, string.Empty);

        // Return success
        return (UserOperationResult.Success, token);
    }

    /// <summary>
    /// Logins a user and returns a result with a token if successful.
    /// </summary>
    /// <param name="userLoginDto"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<(UserOperationResult Result, string Token)> LoginAsync(UserLoginDto userLoginDto, CancellationToken cancellationToken = default)
    {
        var (result, userInfo) = await _userService.GetUserInfoForLogin(userLoginDto);
        if (result != UserOperationResult.Success)
            return (result, string.Empty);

        // Create Token
        var token = _tokenHelper.GenerateToken(
            new UserTokenBaseClaims(userInfo.Id, userInfo.Username, userInfo.Email),
            new List<UserClaim>
            {
                new("RoleId", ((int)userInfo.Role).ToString()),
                new("Name",  userInfo.Name)
            }
        );

        if (string.IsNullOrEmpty(token))
            return (UserOperationResult.TokenGenerationFailed, string.Empty);

        // Return success
        return (UserOperationResult.Success, token);
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Creates a unique user code based on role.
    /// </summary>
    /// <param name="role">The user role.</param>
    /// <returns>A unique code like INS-ABC123.</returns>
    public static string GenerateUserCode(UserRole role)
    {
        string rolePrefix = role switch
        {
            UserRole.Admin => "ADM",
            UserRole.Instructor => "INS",
            UserRole.Student => "STD",
            _ => "USR"
        };

        string uniquePart = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
        return $"{rolePrefix}-{uniquePart}";
    }

    #endregion
}

