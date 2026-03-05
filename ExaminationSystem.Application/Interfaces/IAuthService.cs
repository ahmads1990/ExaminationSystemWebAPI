using ExaminationSystem.Application.DTOs.Auth;

namespace ExaminationSystem.Application.Interfaces;

public interface IAuthService
{
    Task<(UserOperationResult Result, int Id)> RegisterInstructorAsync(RegisterInstructorDto registerInstructorDto, CancellationToken cancellationToken = default);
    Task<(UserOperationResult Result, int Id)> RegisterStudentAsync(RegisterStudentDto registerStudentDto, CancellationToken cancellationToken = default);
    Task<(UserOperationResult Result, UserTokensDto? tokensDto)> LoginAsync(UserLoginDto userLoginDto, CancellationToken cancellationToken = default);
    Task<UserEmailVerificationResult> VerifyEmailAsync(int userId, string token, CancellationToken cancellationToken = default);
    Task<UserEmailVerificationResult> RefreshUserEmailVerificationToken(int userId, CancellationToken cancellationToken = default);
    Task<(UserOperationResult result, UserTokensDto? tokensDto)> RefreshUserToken(int userId, string refreshToken, CancellationToken cancellationToken = default);
    Task<(UserOperationResult result, string token)> CreateExamAttemptToken(CreateExamTokenDto createTokenDto, CancellationToken cancellationToken = default);
}
