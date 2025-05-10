using ExaminationSystem.Application.DTOs.Users;
using ExaminationSystem.Application.InfraInterfaces;
using ExaminationSystem.Infrastructure.Configs;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace ExaminationSystem.Infrastructure.Services.Auth;

public class TokenHelper : ITokenHelper
{
    public string GenerateToken(UserTokenBaseClaims baseUserClaims, List<UserClaim> userClaims)
    {
        // Validate base user claims
        if (baseUserClaims.AreClaimsInValid())
            return string.Empty;

        var jwtClaims = new UserClaim[]
        {
            new UserClaim(JwtRegisteredClaimNames.Name, baseUserClaims.Name ?? string.Empty),
            new UserClaim(JwtRegisteredClaimNames.Email, baseUserClaims.Email ?? string.Empty),
            new UserClaim(JwtRegisteredClaimNames.Sid, baseUserClaims.Uid.ToString())
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
}

