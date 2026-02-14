using ExaminationSystem.Application.DTOs.Users;
using ExaminationSystem.Application.InfraInterfaces;
using ExaminationSystem.Infrastructure.Configs;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using OtpNet;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ExaminationSystem.Infrastructure.Services.Auth;

public class TokenHelper : ITokenHelper
{
    #region Fields

    private const int OTP_LENGTH = 6;
    private const int REFRESH_TOKEN_LENGTH = 32;
    private readonly string SECRET_KEY;

    #endregion

    #region Constructor

    public TokenHelper(IConfiguration configuration)
    {
        SECRET_KEY = configuration.GetSection("OTPSecretKey")?.Value
            ?? throw new InvalidOperationException("Missing required configuration: 'OTPSecretKey'.");
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Generates a JWT token based on the provided user claims.
    /// </summary>
    /// <param name="baseUserClaims">Required claims</param>
    /// <param name="userClaims"></param>
    /// <returns></returns>
    public string GenerateJWT(UserTokenBaseClaims baseUserClaims, List<UserClaim> userClaims)
    {
        // Validate base user claims
        if (baseUserClaims.AreClaimsInValid())
            return string.Empty;

        var jwtClaims = new UserClaim[]
        {
            new UserClaim(ClaimTypes.NameIdentifier, baseUserClaims.Uid.ToString()),
            new UserClaim(ClaimTypes.Role, baseUserClaims.Role.ToString()),
            new UserClaim(ClaimTypes.Name, baseUserClaims.Name ?? string.Empty),
            new UserClaim(ClaimTypes.Email, baseUserClaims.Email ?? string.Empty)
        };

        // Merge base claims and additional user claims
        var allClaims = jwtClaims
            .Union(userClaims)
            .Select(c => new Claim(c.Type, c.Value));

        // Specify the signing key and algorithm
        var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtConfig.Key));
        var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

        // Create the JWT token
        var jwtSecurityToken = new JwtSecurityToken(
            issuer: JwtConfig.Issuer,
            audience: JwtConfig.Audience,
            claims: allClaims,
            expires: DateTime.UtcNow.AddHours(JwtConfig.DurationInHours),
            signingCredentials: signingCredentials
        );

        // Return the serialized token
        return new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
    }

    /// <summary>
    /// Generates a new refresh token as a random string suitable for authentication scenarios.
    /// </summary>
    /// <returns>A string containing the newly generated refresh token.</returns>
    public string GenerateRefreshToken()
    {
        return GenerateOTP(REFRESH_TOKEN_LENGTH);
    }

    /// <summary>
    /// Generates a one-time password (OTP) using the configured secret key and specified length.
    /// </summary>
    /// <returns>A string containing the generated OTP of the specified length.</returns>
    public string GenerateOTP(int length = OTP_LENGTH)
    {
        var totp = new Totp(Encoding.UTF8.GetBytes(SECRET_KEY), totpSize: length);
        return totp.ComputeTotp();
    }

    #endregion
}
