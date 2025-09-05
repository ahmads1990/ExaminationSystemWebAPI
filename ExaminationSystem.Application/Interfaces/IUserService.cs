using ExaminationSystem.Application.DTOs.Users;

namespace ExaminationSystem.Application.Interfaces;

public interface IUserService
{
    Task<(UserOperationResult Result, int Id)> AddAsync(AddUserDto userDto, CancellationToken cancellationToken = default);
}

