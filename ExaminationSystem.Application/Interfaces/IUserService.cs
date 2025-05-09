using ExaminationSystem.Application.DTOs.Users;

namespace ExaminationSystem.Application.Interfaces;

public interface IUserService
{
    Task<(AddUserResult result, int Id)> Add(AddUserDto userDto, CancellationToken cancellationToken = default);
    Task<bool> IsUserEmailUnique(string email, CancellationToken cancellationToken = default);
}

