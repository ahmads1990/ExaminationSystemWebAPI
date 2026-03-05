using ExaminationSystem.Application.DTOs.Users;

namespace ExaminationSystem.Application.InfraInterfaces;

public interface ITokenHelper
{
    string GenerateJWT(UserTokenBaseClaims baseClaims, List<UserClaim> userClaims, int expiresInMinutes = 0);
    string GenerateRefreshToken();
    string GenerateOTP(int length = 6);
}
