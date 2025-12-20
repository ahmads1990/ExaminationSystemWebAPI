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

    /// <summary>
    /// Asynchronously retrieves basic information for a user identified by the specified user ID.
    /// </summary>
    /// <param name="userId">The unique identifier of the user whose basic information is to be retrieved.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A <see cref="UserBasicInfo"/> object containing the user's basic information if found; otherwise, <see
    /// langword="null"/>.</returns>
    public async Task<UserBasicInfo?> GetUserBasicInfoById(int userId, CancellationToken cancellationToken = default)
    {
        return await _userRepository.GetByCondition(u => u.ID == userId)
                                              .ProjectToType<UserBasicInfo>()
                                              .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Confirms the email address for the specified user if it has not already been confirmed.
    /// </summary>
    /// <param name="userId">The unique identifier of the user whose email address is to be confirmed.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A value indicating the result of the email confirmation attempt. Returns <see
    /// cref="UserEmailVerificationResult.Success"/> if the email was successfully confirmed; <see
    /// cref="UserEmailVerificationResult.AlreadyConfirmed"/> if the email was already confirmed; or <see
    /// cref="UserEmailVerificationResult.UserNotFound"/> if the user does not exist.</returns>
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

