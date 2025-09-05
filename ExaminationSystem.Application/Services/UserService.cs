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

    #endregion

    #region Private Methods

    private async Task<bool> IsUserEmailUnique(string email, CancellationToken cancellationToken = default)
    {
        var userExist = await _userRepository.CheckExistsByCondition(u => u.Email.Equals(email), cancellationToken);
        return !userExist;
    }

    #endregion
}

