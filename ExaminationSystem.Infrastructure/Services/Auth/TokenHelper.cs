using ExaminationSystem.Application.Common;
using ExaminationSystem.Application.DTOs.Users;
using ExaminationSystem.Application.InfraInterfaces;
using ExaminationSystem.Infrastructure.Configs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OtpNet;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ExaminationSystem.Infrastructure.Services.Auth;

public class TokenHelper : ITokenHelper
{
    #region Constants

    private const int OTP_LENGTH = 6;
    private const int REFRESH_TOKEN_LENGTH = 32;

    #endregion

    #region Fields

    private readonly string SECRET_KEY;
    private readonly JwtConfig _jwtConfig;

    #endregion

    #region Constructors

    public TokenHelper(IOptions<JwtConfig> jwtOptions, IConfiguration configuration)
    {
        _jwtConfig = jwtOptions.Value;
        SECRET_KEY = configuration.GetSection("OTPSecretKey")?.Value
            ?? throw new InvalidOperationException("Missing required configuration: 'OTPSecretKey'.");
    }

    #endregion

    #region Public Methods

    /// <inheritdoc />
    public string GenerateJWT(UserTokenBaseClaims baseUserClaims, List<UserClaim> userClaims, int expiresInMinutes = 0)
    {
        // Validate base user claims
        if (baseUserClaims.AreClaimsInValid())
            return string.Empty;

        var jwtClaims = new UserClaim[]
        {
            new UserClaim(ClaimTypes.NameIdentifier, baseUserClaims.Uid.ToString()),
            new UserClaim(CustomClaimTypes.TenantId, baseUserClaims.TenantId.ToString()),
            new UserClaim(ClaimTypes.Role, baseUserClaims.Role.ToString()),
            new UserClaim(ClaimTypes.Name, baseUserClaims.Name ?? string.Empty),
            new UserClaim(ClaimTypes.Email, baseUserClaims.Email ?? string.Empty),
            new UserClaim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // Merge base claims and additional user claims
        var allClaims = jwtClaims
            .Union(userClaims)
            .Select(c => new Claim(c.Type, c.Value));

        // Specify the signing key and algorithm
        var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfig.Key));
        var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

        var expirationDate = expiresInMinutes > 0
            ? DateTime.UtcNow.AddMinutes(expiresInMinutes)
            : DateTime.UtcNow.AddHours(_jwtConfig.DurationInHours);

        // Create the JWT token
        var jwtSecurityToken = new JwtSecurityToken(
            issuer: _jwtConfig.Issuer,
            audience: _jwtConfig.Audience,
            claims: allClaims,
            expires: expirationDate,
            signingCredentials: signingCredentials
        );

        // Return the serialized token
        return new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
    }

    /// <inheritdoc />
    public string GenerateRefreshToken()
    {
        return GenerateOTP(REFRESH_TOKEN_LENGTH);
    }

    /// <inheritdoc />
    public string GenerateOTP(int length = OTP_LENGTH)
    {
        var totp = new Totp(Encoding.UTF8.GetBytes(SECRET_KEY), totpSize: length);
        return totp.ComputeTotp();
    }

    #endregion
}
