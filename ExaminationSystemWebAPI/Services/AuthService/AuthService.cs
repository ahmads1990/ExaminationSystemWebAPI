using ExaminationSystemWebAPI.Helpers;
using ExaminationSystemWebAPI.Helpers.Config;
using ExaminationSystemWebAPI.Models.Users;
using ExaminationSystemWebAPI.ViewModels.Auth;
using ExaminationSystemWebAPI.ViewModels.Auth.Register;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ExaminationSystemWebAPI.Services.AuthService;

public class AuthService : IAuthService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly JwtConfig _jwtConfig;

    public AuthService(UserManager<AppUser> userManager, IOptions<JwtConfig> jwtConfig)
    {
        _userManager = userManager;
        _jwtConfig = jwtConfig.Value;
    }

    public async Task<AuthViewModel> RegisterUser(RegisterViewModel registerViewModel)
    {
        // first check if user email is already exists in database
        if (await _userManager.FindByEmailAsync(registerViewModel.Email) is not null)
            return new AuthViewModel { Message = "Email already exists" };

        // check if username is already exists in database
        if (await _userManager.FindByNameAsync(registerViewModel.UserName) is not null)
            return new AuthViewModel { Message = "Username already exists" };

        // create new user object
        var user = new AppUser();
        registerViewModel.Adapt(user);

        var result = await _userManager.CreateAsync(user, registerViewModel.Password); 
        
        if (!result.Succeeded)
        {
            string errorMessage = string.Empty;
            foreach (var error in result.Errors)
            {
                errorMessage += $"{error.Description} | ";
            }
            return new AuthViewModel { Message = errorMessage };
        }

        await _userManager.AddClaimAsync(user, new Claim(registerViewModel.ClaimRole, registerViewModel.ClaimRole));

        // user creation went ok then create token and send it back
        var jwtToken = await CreateJwtTokenAsync(user);

        return new AuthViewModel
        {
            UserID = user.Id,
            IsAuthenticated = true,
            Username = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            Claims = new List<string>() { registerViewModel.ClaimRole },
            Token = new JwtSecurityTokenHandler().WriteToken(jwtToken),
            ExpiresOn = jwtToken?.ValidTo ?? DateTime.Now
        };
    }

    public async Task<AuthViewModel> LoginUser(LoginViewModel loginViewModel)
    {
        AuthViewModel authDto = new AuthViewModel();
        // return if email doesn't exist OR email+password don't match
        var user = await _userManager.FindByEmailAsync(loginViewModel.Email);

        if (user is null || !await _userManager.CheckPasswordAsync(user, loginViewModel.Password))
        {
            authDto.Message = "Email or Password is incorrect!";
            return authDto;
        }

        var jwtToken = await CreateJwtTokenAsync(user);
        var claims = await _userManager.GetClaimsAsync(user);

        authDto.IsAuthenticated = true;
        authDto.UserID = user.Id;
        authDto.Username = user.UserName ?? string.Empty;
        authDto.Email = user.Email ?? string.Empty;
        authDto.Claims = claims.Select(c => c.Type).ToList();
        authDto.Token = new JwtSecurityTokenHandler().WriteToken(jwtToken);
        authDto.ExpiresOn = jwtToken?.ValidTo ?? DateTime.Now;

        return authDto;
    }

    public async Task<string> AddClaim(AddClaimViewModel addClaimViewModel)
    {
        var user = await _userManager.FindByIdAsync(addClaimViewModel.UserId);

        //check first if user with that id exists
        if (user is null)
            return "Invalid user ID";

        // claim type exists in allowed types
        if (!CustomClaimTypes.ALLOWEDTYPES.Contains(addClaimViewModel.ClaimType))
            return "Invalid claim type not allowed";

        // check user claims to see if user has this claim already
        var claims = await _userManager.GetClaimsAsync(user);
        if (claims.FirstOrDefault(c => c.Type.Equals(addClaimViewModel.ClaimType)) != null)
            return "User already assigned to this claim";

        // try to add the claim to user
        var result = await _userManager.AddClaimAsync(user, new Claim(addClaimViewModel.ClaimType, addClaimViewModel.ClaimValue));

        return result.Succeeded ? string.Empty : "Something went wrong";
    }

    private async Task<JwtSecurityToken?> CreateJwtTokenAsync(AppUser user)
    {
        if (user is null) return null;
        // get user claims
        var userClaims = await _userManager.GetClaimsAsync(user);
        // create jwt claims
        var jwtClaims = new[]
        {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim("uid", user.Id)
        };
        // merge both claims lists and jwtClaims to allClaims
        var allClaims = jwtClaims.Union(userClaims);

        // specify the signing key and algorithm
        var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfig.Key));
        var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

        // finally create the token
        var jwtSecurityToken = new JwtSecurityToken(
            issuer: _jwtConfig.Issuer,
            audience: _jwtConfig.Audience,
            claims: allClaims,
            expires: DateTime.Now.AddHours(_jwtConfig.DurationInHours),
            signingCredentials: signingCredentials
            );

        return jwtSecurityToken;
    }
}
