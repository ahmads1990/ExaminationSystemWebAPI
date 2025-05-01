using ExaminationSystem.Application.DTOs.Users;
using ExaminationSystem.Application.Interfaces;
using ExaminationSystem.Domain.Entities;
using ExaminationSystem.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ExaminationSystem.Application.Services;

public class UserService : IUserService
{
    private readonly IRepository<AppUser> _userRepository;

    public UserService(IRepository<AppUser> userRepository)
    {
        _userRepository = userRepository;
    }

    public Task Add(AddUserDto userDto, CancellationToken cancellationToken = default)
    {
        var user = userDto.Adapt<AppUser>();
        _userRepository.Add(user);
    }

    public async Task<bool> IsUserEmailUnique(string email, CancellationToken cancellationToken = default)
    {
        var userExist = await _userRepository.GetByCondition(u => u.Email.Equals(email)).AnyAsync(cancellationToken);
        return !userExist;
    }
}

