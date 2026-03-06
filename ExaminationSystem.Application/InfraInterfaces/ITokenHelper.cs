using ExaminationSystem.Application.DTOs.Users;

namespace ExaminationSystem.Application.InfraInterfaces;

/// <summary>
/// Provides methods for generating tokens and OTPs.
/// </summary>
public interface ITokenHelper
{
    /// <summary>
    /// Generates a JSON Web Token (JWT) for a user.
    /// </summary>
    /// <param name="baseClaims">Basic user claims.</param>
    /// <param name="userClaims">Additional custom user claims.</param>
    /// <param name="expiresInMinutes">Token expiration time in minutes.</param>
    /// <returns>A signed JWT string.</returns>
    string GenerateJWT(UserTokenBaseClaims baseClaims, List<UserClaim> userClaims, int expiresInMinutes = 0);

    /// <summary>
    /// Generates a secure random refresh token.
    /// </summary>
    /// <returns>A base64 encoded refresh token string.</returns>
    string GenerateRefreshToken();

    /// <summary>
    /// Generates a numeric one-time password (OTP).
    /// </summary>
    /// <param name="length">The length of the OTP.</param>
    /// <returns>A numeric OTP string.</returns>
    string GenerateOTP(int length = 6);
}
