using ExaminationSystem.Application.DTOs.Users;

namespace ExaminationSystem.Application.Interfaces;

public interface IUserService
{
    Task Add(AddUserDto userDto, CancellationToken cancellationToken = default);
    Task<bool> IsUserEmailUnique(string email, CancellationToken cancellationToken = default);
}

