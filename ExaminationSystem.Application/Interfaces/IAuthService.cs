using ExaminationSystem.Application.DTOs.Auth;

namespace ExaminationSystem.Application.Interfaces;

public interface IAuthService
{
    Task<(UserOperationResult Result, string Token)> RegisterInstructorAsync(RegisterInstructorDto registerInstructorDto, CancellationToken cancellationToken = default);
}

