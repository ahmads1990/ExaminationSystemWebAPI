using ExaminationSystem.Application.DTOs.Users;

namespace ExaminationSystem.Application.InfraInterfaces;

public interface ITokenHelper
{
    string GenerateToken(UserTokenBaseClaims baseClaims, List<UserClaim> userClaims);
}


