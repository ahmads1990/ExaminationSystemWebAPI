using ExaminationSystem.Application.DTOs.Auth;
using ExaminationSystem.Application.DTOs.Users;
using ExaminationSystem.Application.InfraInterfaces;
using ExaminationSystem.Application.Interfaces;
using ExaminationSystem.Domain.Entities;
using ExaminationSystem.Domain.Interfaces;

namespace ExaminationSystem.Application.Services;

public class UserService : IUserService
{
    private readonly IRepository<AppUser> _userRepository;
    private readonly IPasswordHelper _passwordHelper;

    public UserService(IRepository<AppUser> userRepository, IPasswordHelper passwordHelper)
    {
        _userRepository = userRepository;
        _passwordHelper = passwordHelper;
    }

    #region Public Methods

    /// <summary>
    /// Adds a new user to the system after validating the input and ensuring email uniqueness.
    /// </summary>
    /// <param name="userDto"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<(UserOperationResult Result, int Id)> AddAsync(AddUserDto userDto, CancellationToken cancellationToken = default)
    {
        // Validate required fields
        if (string.IsNullOrEmpty(userDto.Name) ||
            string.IsNullOrEmpty(userDto.Username) ||
            string.IsNullOrEmpty(userDto.Email) ||
            string.IsNullOrEmpty(userDto.Password))
        {
            return (UserOperationResult.ValidationFailed, 0);
        }

        // Check email is unique
        var isEmailUnique = await IsUserEmailUnique(userDto.Email, cancellationToken);
        if (!isEmailUnique)
            return (UserOperationResult.EmailDuplicated, 0);

        var user = userDto.Adapt<AppUser>();

        // Hash password
        var hashedPassword = _passwordHelper.HashPassword(user.Password);
        user.Password = hashedPassword;

        await _userRepository.Add(user, cancellationToken);
        await _userRepository.SaveChanges(cancellationToken);

        return (UserOperationResult.Success, user.ID);
    }

    /// <summary>
    /// Gets user information for login by validating credentials.
    /// </summary>
    /// <param name="userLoginDto"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<(UserOperationResult Result, UserBasicInfo? UserInfo)> GetUserInfoForLogin(UserLoginDto userLoginDto, CancellationToken cancellationToken = default)
    {
        // Hash the provided password
        var hashedPassword = _passwordHelper.HashPassword(userLoginDto.Password);

        // Find user by email and hashed password
        var user = await _userRepository.GetByCondition(u => u.Email == userLoginDto.Email && u.Password == hashedPassword, cancellationToken);
        if (user == null)
            return (UserOperationResult.InvalidCredentials, null);

        var userInfo = user.Adapt<UserBasicInfo>();
        return (UserOperationResult.Success, userInfo);
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// verifies if the provided email is unique in the DB.
    /// </summary>
    /// <param name="email"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<bool> IsUserEmailUnique(string email, CancellationToken cancellationToken = default)
    {
        var userExist = await _userRepository.CheckExistsByCondition(u => u.Email.Equals(email), cancellationToken);
        return !userExist;
    }

    #endregion
}

