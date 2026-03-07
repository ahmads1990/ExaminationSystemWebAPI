using ExaminationSystem.Application.Common;
using ExaminationSystem.Application.DTOs.Auth;
using ExaminationSystem.Application.DTOs.Instructor;
using ExaminationSystem.Application.DTOs.Student;
using ExaminationSystem.Application.DTOs.Users;
using ExaminationSystem.Application.InfraInterfaces;
using ExaminationSystem.Application.Interfaces;
using ExaminationSystem.Application.UseCases;
using ExaminationSystem.Domain.Entities;
using ExaminationSystem.Domain.Interfaces;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ExaminationSystem.Application.Services;

public class AuthService : IAuthService
{
    #region Constants

    private const string CacheEmailConfirmationKey = "user:email_confirmation:";

    #endregion

    #region Fields

    private readonly IUserService _userService;
    private readonly IInstructorService _instructorService;
    private readonly IStudentService _studentService;
    private readonly ITokenHelper _tokenHelper;
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly ICachingService _cachingService;
    private readonly IPasswordHelper _passwordHelper;
    private readonly IRepository<RefreshToken> _refreshTokenRepo;

    private readonly string BackendBaseUrl;
    private readonly int RefreshTokenLifeInDays;

    #endregion

    #region Constructors

    public AuthService(IUserService userService, IInstructorService instructorService, IStudentService studentService,
        ITokenHelper tokenHelper, IBackgroundJobClient backgroundJobClient, ICachingService cachingService,
        IRepository<RefreshToken> refreshTokenRepo, IPasswordHelper passwordHelper, IConfiguration configuration)
    {
        _userService = userService;
        _instructorService = instructorService;
        _studentService = studentService;
        _tokenHelper = tokenHelper;
        _backgroundJobClient = backgroundJobClient;
        _cachingService = cachingService;
        _refreshTokenRepo = refreshTokenRepo;
        _passwordHelper = passwordHelper;

        BackendBaseUrl = configuration.GetSection("BackendBaseUrl").Value
            ?? throw new InvalidOperationException("Missing required configuration: 'BackendBaseUrl'.");

        if (!int.TryParse(configuration.GetSection("Jwt:RefreshTokenLifeInDays").Value, out int parsedResult))
            throw new InvalidOperationException("Missing or invalid configuration: 'Jwt:RefreshTokenLifeInDays' must be a positive integer.");

        RefreshTokenLifeInDays = parsedResult;
    }

    #endregion

    #region Public Methods

    /// <inheritdoc />
    public async Task<(UserOperationResult Result, int Id)> RegisterInstructorAsync(RegisterInstructorDto registerInstructorDto, CancellationToken cancellationToken = default)
    {
        // Prepare user dto
        var addUserDto = registerInstructorDto.Adapt<AddUserDto>();
        addUserDto.Role = UserRole.Instructor;
        addUserDto.Code = GenerateUserCode(UserRole.Instructor);

        // Save user
        var (addUserResult, userId) = await _userService.AddAsync(addUserDto, cancellationToken);
        if (addUserResult != UserOperationResult.Success)
            return (addUserResult, 0);

        // Prepare instructor dto
        var addInstructorDto = registerInstructorDto.Adapt<AddInstructorDto>();
        addInstructorDto.ID = userId;

        // Save instructor
        var addInstructorResult = await _instructorService.AddAsync(addInstructorDto, cancellationToken);
        if (addInstructorResult != UserOperationResult.Success)
            return (addInstructorResult, 0);

        var userOtp = await CreateEmailConfirmationToken(userId);

        // Add job to send welcome email
        EnqueueSendWelcomeEmailJob(userId, addUserDto.Name, addUserDto.Email, userOtp);

        // Return success
        return (UserOperationResult.Success, userId);
    }

    /// <inheritdoc />
    public async Task<(UserOperationResult Result, int Id)> RegisterStudentAsync(RegisterStudentDto registerStudentDto, CancellationToken cancellationToken = default)
    {
        // Prepare user dto
        var addUserDto = registerStudentDto.Adapt<AddUserDto>();
        addUserDto.Role = UserRole.Student;
        addUserDto.Code = GenerateUserCode(UserRole.Student);

        // Save user
        var (addUserResult, userId) = await _userService.AddAsync(addUserDto, cancellationToken);
        if (addUserResult != UserOperationResult.Success)
            return (addUserResult, 0);

        // Prepare student dto
        var addStudentDto = registerStudentDto.Adapt<AddStudentDto>();
        addStudentDto.ID = userId;

        // Save student
        var addStudentResult = await _studentService.AddAsync(addStudentDto, cancellationToken);
        if (addStudentResult != UserOperationResult.Success)
            return (addStudentResult, 0);

        var userOtp = await CreateEmailConfirmationToken(userId);

        // Add job to send welcome email
        EnqueueSendWelcomeEmailJob(userId, addUserDto.Name, addUserDto.Email, userOtp);

        // Return success
        return (UserOperationResult.Success, userId);
    }

    /// <inheritdoc />
    public async Task<(UserOperationResult Result, UserTokensDto? tokensDto)> LoginAsync(UserLoginDto userLoginDto, CancellationToken cancellationToken = default)
    {
        var (result, userId) = await _userService.VerifyUserPassword(userLoginDto, cancellationToken);
        if (result != UserOperationResult.Success)
            return (result, null);

        var jwtToken = await GenerateUserJWT(userId!.Value, cancellationToken: cancellationToken);
        if (string.IsNullOrEmpty(jwtToken))
            return (UserOperationResult.TokenGenerationFailed, null);

        var newRefreshToken = await GenerateRefreshToken(userId!.Value, cancellationToken: cancellationToken);
        return (UserOperationResult.Success, new UserTokensDto { JwtToken = jwtToken, RefreshToken = newRefreshToken });
    }

    /// <inheritdoc />
    public async Task<UserEmailVerificationResult> VerifyEmailAsync(int userId, string token, CancellationToken cancellationToken = default)
    {
        var userOtp = await _cachingService.GetAsync(CacheEmailConfirmationKey + userId, cancellationToken);

        if (string.IsNullOrEmpty(userOtp))
            return UserEmailVerificationResult.TokenExpired;

        if (userOtp != token)
            return UserEmailVerificationResult.InvalidToken;

        var result = await _userService.ConfirmUserEmail(userId, cancellationToken);
        if (result != UserEmailVerificationResult.Success)
            return result;

        await _cachingService.RemoveAsync(CacheEmailConfirmationKey + userId, cancellationToken);

        return UserEmailVerificationResult.Success;
    }

    /// <inheritdoc />
    public async Task<UserEmailVerificationResult> RefreshUserEmailVerificationToken(int userId, CancellationToken cancellationToken = default)
    {
        // Simply remove the existing token from cache to allow regeneration
        await _cachingService.RemoveAsync(CacheEmailConfirmationKey + userId, cancellationToken);

        var userData = await _userService.GetUserBasicInfoById(userId, cancellationToken);
        if (userData == null)
            return UserEmailVerificationResult.UserNotFound;

        var newToken = await CreateEmailConfirmationToken(userId, cancellationToken);

        EnqueueSendWelcomeEmailJob(userId, userData.Name, userData.Email, newToken);
        return UserEmailVerificationResult.EmailJobSent;
    }

    /// <inheritdoc />
    public async Task<(UserOperationResult result, UserTokensDto? tokensDto)> RefreshUserToken(int userId, string refreshToken, CancellationToken cancellationToken = default)
    {
        var storedRefreshToken = await _refreshTokenRepo
            .GetByCondition(rt => rt.UserId == userId && !rt.IsRevoked)
            .FirstOrDefaultAsync(cancellationToken);

        if (storedRefreshToken is null)
            return (UserOperationResult.InvalidRefreshToken, null);

        if (storedRefreshToken.ExpirationDate < DateTime.UtcNow)
            return (UserOperationResult.ExpiredRefreshToken, null);

        var hashedToken = _passwordHelper.HashPassword(refreshToken);
        if (storedRefreshToken.TokenHash != hashedToken)
            return (UserOperationResult.InvalidRefreshToken, null);

        var jwtToken = await GenerateUserJWT(userId, cancellationToken: cancellationToken);
        if (string.IsNullOrEmpty(jwtToken))
            return (UserOperationResult.TokenGenerationFailed, null);

        var newRefreshToken = await GenerateRefreshToken(userId, storedRefreshToken.ID, cancellationToken);
        return (UserOperationResult.Success, new UserTokensDto { JwtToken = jwtToken, RefreshToken = newRefreshToken });
    }

    /// <inheritdoc />
    public async Task<(UserOperationResult result, string token)> CreateExamAttemptToken(CreateExamTokenDto createTokenDto, CancellationToken cancellationToken = default)
    {
        var extraClaims = new List<UserClaim>
        {
            new(CustomClaimTypes.ExamAttemptId, createTokenDto.ExamAttemptId.ToString()),
            new(CustomClaimTypes.ExamStartDate, DateTime.UtcNow.ToString("O")),
            new(CustomClaimTypes.ExamDeadline, DateTime.UtcNow.AddMinutes(createTokenDto.MaxDurationInMinutes).ToString("O")),
            new(CustomClaimTypes.Scope, ScopeNames.ExamAnswer)
        };

        var jwtToken = await GenerateUserJWT(createTokenDto.StudentId, extraClaims, createTokenDto.MaxDurationInMinutes, cancellationToken);
        return string.IsNullOrEmpty(jwtToken)
               ? (UserOperationResult.TokenGenerationFailed, string.Empty)
               : (UserOperationResult.Success, jwtToken);
    }

    /// <inheritdoc />
    public async Task<UserOperationResult> LogoutAsync(int userId, string jti, DateTime expirationDate, CancellationToken cancellationToken = default)
    {
        // 1. Revoke the active refresh token in the database
        var storedRefreshToken = await _refreshTokenRepo
            .GetByCondition(rt => rt.UserId == userId && !rt.IsRevoked)
            .FirstOrDefaultAsync(cancellationToken);

        if (storedRefreshToken is not null)
        {
            storedRefreshToken.IsRevoked = true;
            _refreshTokenRepo.Update(storedRefreshToken);
            await _refreshTokenRepo.SaveChanges(cancellationToken);
        }

        // 2. Blacklist the JTI in Redis cache until it expires naturally
        var expiryTime = expirationDate - DateTime.UtcNow;
        if (expiryTime > TimeSpan.Zero)
        {
            var cacheKey = $"blacklist:jti:{jti}";
            await _cachingService.AddAsync(cacheKey, "revoked", expiryTime, cancellationToken);
        }

        return UserOperationResult.Success;
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Generates a one-time email confirmation token for the specified user and stores it in the cache for later
    /// verification.
    /// </summary>
    /// <param name="userId">The unique identifier of the user for whom the email confirmation token is generated.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A string containing the generated email confirmation token.</returns>
    private async Task<string> CreateEmailConfirmationToken(int userId, CancellationToken cancellationToken = default)
    {
        var otp = _tokenHelper.GenerateOTP();
        await _cachingService.AddAsync(CacheEmailConfirmationKey + userId, otp, TimeSpan.FromMinutes(5), cancellationToken);
        return otp;
    }

    /// <summary>
    /// Enqueues a background job to send a welcome email to the specified recipient.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="toName">The display name of the recipient to whom the welcome email will be addressed. Cannot be null or empty.</param>
    /// <param name="toEmail">The email address of the recipient who will receive the welcome email. Cannot be null or empty.</param>
    /// <param name="otpCode">The verification code.</param>
    private void EnqueueSendWelcomeEmailJob(int userId, string toName, string toEmail, string otpCode)
    {
        var baseUrl = $"{BackendBaseUrl.TrimEnd('/')}/VerifyEmail/token={otpCode}&userId={userId}";
        var emailParameters = new Dictionary<string, string>
        {
            { "UserName", toName },
            { "ConfirmationLink", baseUrl},
            { "VerificationCode", otpCode},
            { "Year", DateTime.Now.Year.ToString()},
        };

        _backgroundJobClient.Enqueue<SendEmailJob>(job =>
            job.Execute(toName, toEmail, "Welcome new user", EmailTemplate.Welcome, emailParameters, default)
        );
    }

    /// <summary>
    /// Creates a unique user code based on role.
    /// </summary>
    /// <param name="role">The user role.</param>
    /// <returns>A unique code like INS-ABC123.</returns>
    private static string GenerateUserCode(UserRole role)
    {
        string rolePrefix = role switch
        {
            UserRole.Admin => "ADM",
            UserRole.Instructor => "INS",
            UserRole.Student => "STD",
            _ => "USR"
        };

        string uniquePart = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
        return $"{rolePrefix}-{uniquePart}";
    }

    /// <summary>
    /// Generates a JWT access token for the specified user.
    /// </summary>
    /// <param name="userId">The ID of the user to generate the token for.</param>
    /// <param name="extraClaims">Optional extra claims to include in the token.</param>
    /// <param name="expiresInMinutes">Token expiration time in minutes.</param>
    /// <param name="cancellationToken">A cancellation token for the async operation.</param>
    /// <returns>The generated JWT string, or empty if the user's claims are invalid.</returns>
    private async Task<string> GenerateUserJWT(int userId, List<UserClaim>? extraClaims = default, int expiresInMinutes = 0, CancellationToken cancellationToken = default)
    {
        var userInfo = await _userService.GetUserBasicInfoById(userId, cancellationToken);
        var userClaims = new List<UserClaim> { new(CustomClaimTypes.Username, userInfo!.Username) };

        return _tokenHelper.GenerateJWT(
            new UserTokenBaseClaims(userInfo!.ID, userInfo.Role, userInfo.Name, userInfo.Email),
            userClaims.Concat(extraClaims ?? Enumerable.Empty<UserClaim>()).ToList(),
            expiresInMinutes
        );
    }

    /// <summary>
    /// Generates a new refresh token, hashes it, and persists it to the database.
    /// If a <paramref name="tokenId"/> is provided, the existing token record is updated; otherwise a new record is created.
    /// </summary>
    /// <param name="userId">The ID of the user who owns the refresh token.</param>
    /// <param name="tokenId">Optional ID of an existing token record to update (for token rotation).</param>
    /// <param name="cancellationToken">A cancellation token for the async operation.</param>
    /// <returns>The raw (unhashed) refresh token to return to the client.</returns>
    private async Task<string> GenerateRefreshToken(int userId, int? tokenId = null, CancellationToken cancellationToken = default)
    {
        var refreshToken = _tokenHelper.GenerateRefreshToken();
        var hashedToken = _passwordHelper.HashPassword(refreshToken);

        var entity = new RefreshToken
        {
            TokenHash = hashedToken,
            UserId = userId,
            ExpirationDate = DateTime.UtcNow.AddDays(RefreshTokenLifeInDays),
        };

        if (tokenId.HasValue)
        {
            entity.ID = tokenId.Value;
            _refreshTokenRepo.SaveInclude(entity, nameof(RefreshToken.ExpirationDate), nameof(RefreshToken.TokenHash));
        }
        else
        {
            await _refreshTokenRepo.Add(entity, cancellationToken);
        }

        await _refreshTokenRepo.SaveChanges(cancellationToken);
        return refreshToken;
    }

    #endregion
}

