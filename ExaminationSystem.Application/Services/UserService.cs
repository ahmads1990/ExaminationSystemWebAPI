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

    public async Task<(AddUserResult result, int Id)> Add(AddUserDto userDto, CancellationToken cancellationToken = default)
    {
        // Validate required fields
        if (string.IsNullOrEmpty(userDto.Email) || string.IsNullOrEmpty(userDto.Password))
        {
            return (AddUserResult.ValidationFailed, 0);
        }

        // Validate email format
        // Validate password strength

        // Check email is unique
        var isEmailUnique = await IsUserEmailUnique(userDto.Email);
        if (!isEmailUnique)
            return (AddUserResult.EmailDuplicated, 0);

        var user = userDto.Adapt<AppUser>();

        // Hash password
        var hashedPassword = _passwordHelper.HashPassword(user.Password);
        user.Password = hashedPassword;

        await _userRepository.Add(user, cancellationToken);
        await _userRepository.SaveChanges(cancellationToken);

        return (AddUserResult.Success, user.ID);
    }

    public async Task<bool> IsUserEmailUnique(string email, CancellationToken cancellationToken = default)
    {
        var userExist = await _userRepository.GetByCondition(u => u.Email.Equals(email)).AnyAsync(cancellationToken);
        return !userExist;
    }
}

