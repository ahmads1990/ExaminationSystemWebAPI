using ExaminationSystem.Application.DTOs.Auth;
using ExaminationSystem.Application.DTOs.Users;

namespace ExaminationSystem.Application.Interfaces;

public interface IUserService
{
    Task<(UserOperationResult Result, int Id)> AddAsync(AddUserDto userDto, CancellationToken cancellationToken = default);

    Task<(UserOperationResult Result, int? Id)> VerifyUserPassword(UserLoginDto userLoginDto, CancellationToken cancellationToken = default);

    Task<UserBasicInfoDto?> GetUserBasicInfoById(int userId, CancellationToken cancellationToken = default);

    Task<UserEmailVerificationResult> ConfirmUserEmail(int userId, CancellationToken cancellationToken = default);
}
