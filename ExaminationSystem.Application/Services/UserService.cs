using ExaminationSystem.Application.DTOs.Auth;
using ExaminationSystem.Application.DTOs.Users;
using ExaminationSystem.Application.InfraInterfaces;
using ExaminationSystem.Application.Interfaces;
using ExaminationSystem.Domain.Entities;
using ExaminationSystem.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ExaminationSystem.Application.Services;

public class UserService : IUserService
{
    #region Fields

    private readonly IRepository<AppUser> _userRepository;
    private readonly IPasswordHelper _passwordHelper;

    #endregion

    #region Constructors

    public UserService(IRepository<AppUser> userRepository, IPasswordHelper passwordHelper)
    {
        _userRepository = userRepository;
        _passwordHelper = passwordHelper;
    }

    #endregion

    #region Public Methods

    /// <inheritdoc />
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

    /// <inheritdoc />
    public async Task<(UserOperationResult Result, int? Id)> VerifyUserPassword(UserLoginDto userLoginDto, CancellationToken cancellationToken = default)
    {
        // Find user by email and hashed password
        var userInfo = await _userRepository.GetByCondition(u => u.Email == userLoginDto.Email)
                                            .Select(u => new { u.ID, u.Password })
                                            .FirstOrDefaultAsync(cancellationToken);

        return _passwordHelper.VerifyPassword(userInfo?.Password ?? "", userLoginDto.Password)
          ? (UserOperationResult.Success, userInfo!.ID)
          : (UserOperationResult.InvalidCredentials, null);
    }

    /// <inheritdoc />
    public async Task<UserBasicInfoDto?> GetUserBasicInfoById(int userId, CancellationToken cancellationToken = default)
    {
        return await _userRepository.GetByCondition(u => u.ID == userId)
                                              .ProjectToType<UserBasicInfoDto>()
                                              .FirstOrDefaultAsync();
    }

    /// <inheritdoc />
    public async Task<UserBasicInfoDto?> GetUserBasicInfoByEmail(string email, CancellationToken cancellationToken = default)
    {
        return await _userRepository.GetByCondition(u => u.Email == email)
                                              .ProjectToType<UserBasicInfoDto>()
                                              .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<UserEmailVerificationResult> ConfirmUserEmail(int userId, CancellationToken cancellationToken = default)
    {
        var userData = await _userRepository.GetByCondition(u => u.ID == userId)
                                        .Select(u => new { ID = u.ID, IsEmailConfirmed = u.IsEmailConfirmed })
                                        .FirstOrDefaultAsync(cancellationToken);

        if (userData == null)
            return UserEmailVerificationResult.UserNotFound;

        if (userData.IsEmailConfirmed)
            return UserEmailVerificationResult.AlreadyConfirmed;

        var user = new AppUser
        {
            ID = userData.ID,
            IsEmailConfirmed = true
        };

        _userRepository.SaveInclude(user, nameof(AppUser.IsEmailConfirmed));
        await _userRepository.SaveChanges(cancellationToken);
        return UserEmailVerificationResult.Success;
    }

    /// <inheritdoc />
    public async Task<UserOperationResult> UpdatePassword(int userId, string newPassword, CancellationToken cancellationToken = default)
    {
        var userExists = await _userRepository.CheckExistsByID(userId, cancellationToken);
        if (!userExists)
            return UserOperationResult.UserNotFound;

        var hashedPassword = _passwordHelper.HashPassword(newPassword);

        var user = new AppUser
        {
            ID = userId,
            Password = hashedPassword
        };

        _userRepository.SaveInclude(user, nameof(AppUser.Password));
        await _userRepository.SaveChanges(cancellationToken);

        return UserOperationResult.Success;
    }

    /// <inheritdoc />
    public async Task<UserOperationResult> ChangePassword(int userId, string oldPassword, string newPassword, CancellationToken cancellationToken = default)
    {
        var userInfo = await _userRepository.GetByCondition(u => u.ID == userId)
                                            .Select(u => new { u.ID, u.Password })
                                            .FirstOrDefaultAsync(cancellationToken);

        if (userInfo == null)
            return UserOperationResult.UserNotFound;

        if (!_passwordHelper.VerifyPassword(userInfo.Password ?? "", oldPassword))
            return UserOperationResult.InvalidCredentials;

        var hashedPassword = _passwordHelper.HashPassword(newPassword);

        var user = new AppUser
        {
            ID = userId,
            Password = hashedPassword
        };

        _userRepository.SaveInclude(user, nameof(AppUser.Password));
        await _userRepository.SaveChanges(cancellationToken);

        return UserOperationResult.Success;
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Verifies if the provided email is unique in the database.
    /// </summary>
    /// <param name="email">The email to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the email is unique, otherwise false.</returns>
    private async Task<bool> IsUserEmailUnique(string email, CancellationToken cancellationToken = default)
    {
        var userExist = await _userRepository.CheckExistsByCondition(u => u.Email.Equals(email), cancellationToken);
        return !userExist;
    }

    #endregion
}

