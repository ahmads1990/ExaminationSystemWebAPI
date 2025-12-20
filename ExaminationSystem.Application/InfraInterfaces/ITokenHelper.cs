using ExaminationSystem.Application.DTOs.Users;

namespace ExaminationSystem.Application.InfraInterfaces;

public interface ITokenHelper
{
    string GenerateJWT(UserTokenBaseClaims baseClaims, List<UserClaim> userClaims);
    string GenerateOTP(int length = 6);
}
