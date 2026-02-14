using ExaminationSystem.Application.DTOs.Auth;
using ExaminationSystem.Application.DTOs.Instructor;
using ExaminationSystem.Application.DTOs.Student;
using ExaminationSystem.Application.DTOs.Users;
using ExaminationSystem.Application.InfraInterfaces;
using ExaminationSystem.Application.Interfaces;
using ExaminationSystem.Application.Services;
using ExaminationSystem.Domain.Entities;
using ExaminationSystem.Domain.Interfaces;
using FluentAssertions;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using Microsoft.Extensions.Configuration;
using Moq;

namespace ExaminationSystem.UnitTests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserService> _userServiceMock;
    private readonly Mock<IInstructorService> _instructorServiceMock;
    private readonly Mock<IStudentService> _studentServiceMock;
    private readonly Mock<ITokenHelper> _tokenHelperMock;
    private readonly Mock<IBackgroundJobClient> _backgroundJobClientMock;
    private readonly Mock<ICachingService> _cachingServiceMock;
    private readonly Mock<IRepository<RefreshToken>> _refreshTokenRepo;
    private readonly Mock<IPasswordHelper> _passwordHelper;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _userServiceMock = new Mock<IUserService>();
        _instructorServiceMock = new Mock<IInstructorService>();
        _studentServiceMock = new Mock<IStudentService>();
        _tokenHelperMock = new Mock<ITokenHelper>();
        _backgroundJobClientMock = new Mock<IBackgroundJobClient>();
        _cachingServiceMock = new Mock<ICachingService>();
        _refreshTokenRepo = new Mock<IRepository<RefreshToken>>();
        _passwordHelper = new Mock<IPasswordHelper>();
        _configurationMock = new Mock<IConfiguration>();

        // Setup configuration mock
        var configSectionMock = new Mock<IConfigurationSection>();
        configSectionMock.Setup(x => x.Value).Returns("https://example.com");
        _configurationMock.Setup(x => x.GetSection("BackendBaseUrl")).Returns(configSectionMock.Object);

        _authService = new AuthService(
            _userServiceMock.Object,
            _instructorServiceMock.Object,
            _studentServiceMock.Object,
            _tokenHelperMock.Object,
            _backgroundJobClientMock.Object,
            _cachingServiceMock.Object,
            _refreshTokenRepo.Object,
            _passwordHelper.Object,
            _configurationMock.Object
        );
    }

    #region RegisterInstructor Tests

    [Fact]
    [Trait("Category", TestCategories.Happy)]
    public async Task RegisterInstructorAsync_ValidData_ReturnsSuccessWithUserId()
    {
        // Arrange
        var dto = new RegisterInstructorDto
        {
            Name = "Test Instructor",
            Username = "testinstructor",
            Email = "test@domain.com",
            Password = "password"
        };
        var cancellationToken = new CancellationToken();

        _userServiceMock.Setup(x => x.AddAsync(It.IsAny<AddUserDto>(), cancellationToken))
            .ReturnsAsync((UserOperationResult.Success, 42));

        _instructorServiceMock.Setup(x => x.AddAsync(It.IsAny<AddInstructorDto>(), cancellationToken))
            .ReturnsAsync(UserOperationResult.Success);

        _tokenHelperMock.Setup(x => x.GenerateOTP(6)).Returns("123456");

        _cachingServiceMock.Setup(x => x.AddAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<TimeSpan>(),
            cancellationToken))
            .Returns(Task.CompletedTask);

        // Act
        var (result, id) = await _authService.RegisterInstructorAsync(dto, cancellationToken);

        // Assert
        result.Should().Be(UserOperationResult.Success);
        id.Should().Be(42);
        _tokenHelperMock.Verify(x => x.GenerateOTP(6), Times.Once);
        _cachingServiceMock.Verify(x => x.AddAsync(
            "user:email_confirmation:42",
            "123456",
            TimeSpan.FromMinutes(5),
            cancellationToken), Times.Once);
        _backgroundJobClientMock.Verify(x => x.Create(
            It.IsAny<Job>(),
            It.IsAny<IState>()), Times.Once);
    }

    [Theory]
    [InlineData(UserOperationResult.ValidationFailed)]
    [InlineData(UserOperationResult.EmailDuplicated)]
    [InlineData(UserOperationResult.UserCreationFailed)]
    [Trait("Category", TestCategories.Validation)]
    public async Task RegisterInstructorAsync_UserCreationFails_ReturnsFailureWithZeroId(UserOperationResult userResult)
    {
        // Arrange
        var dto = new RegisterInstructorDto
        {
            Name = "Test Instructor",
            Username = "testinstructor",
            Email = "test@domain.com",
            Password = "password"
        };
        var cancellationToken = new CancellationToken();

        _userServiceMock.Setup(x => x.AddAsync(It.IsAny<AddUserDto>(), cancellationToken))
            .ReturnsAsync((userResult, 0));

        // Act
        var (result, id) = await _authService.RegisterInstructorAsync(dto, cancellationToken);

        // Assert
        result.Should().Be(userResult);
        id.Should().Be(0);
        _instructorServiceMock.Verify(x => x.AddAsync(It.IsAny<AddInstructorDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(UserOperationResult.ValidationFailed)]
    [InlineData(UserOperationResult.InvalidUserId)]
    [InlineData(UserOperationResult.UserCreationFailed)]
    [Trait("Category", TestCategories.Validation)]
    public async Task RegisterInstructorAsync_InstructorCreationFails_ReturnsFailureWithZeroId(UserOperationResult instructorResult)
    {
        // Arrange
        var dto = new RegisterInstructorDto
        {
            Name = "Test Instructor",
            Username = "testinstructor",
            Email = "test@domain.com",
            Password = "password"
        };
        var cancellationToken = new CancellationToken();

        _userServiceMock.Setup(x => x.AddAsync(It.IsAny<AddUserDto>(), cancellationToken))
            .ReturnsAsync((UserOperationResult.Success, 42));

        _instructorServiceMock.Setup(x => x.AddAsync(It.IsAny<AddInstructorDto>(), cancellationToken))
            .ReturnsAsync(instructorResult);

        // Act
        var (result, id) = await _authService.RegisterInstructorAsync(dto, cancellationToken);

        // Assert
        result.Should().Be(instructorResult);
        id.Should().Be(0);
        _tokenHelperMock.Verify(x => x.GenerateOTP(6), Times.Never);
        _backgroundJobClientMock.Verify(x => x.Create(It.IsAny<Job>(), It.IsAny<IState>()), Times.Never);
    }

    #endregion

    #region RegisterStudent Tests

    [Fact]
    [Trait("Category", TestCategories.Happy)]
    public async Task RegisterStudentAsync_ValidData_ReturnsSuccessWithUserId()
    {
        // Arrange
        var dto = new RegisterStudentDto
        {
            Name = "Test Student",
            Username = "teststudent",
            Email = "student@domain.com",
            Password = "password"
        };
        var cancellationToken = new CancellationToken();

        _userServiceMock.Setup(x => x.AddAsync(It.IsAny<AddUserDto>(), cancellationToken))
            .ReturnsAsync((UserOperationResult.Success, 42));

        _studentServiceMock.Setup(x => x.AddAsync(It.IsAny<AddStudentDto>(), cancellationToken))
            .ReturnsAsync(UserOperationResult.Success);

        _tokenHelperMock.Setup(x => x.GenerateOTP(6)).Returns("123456");

        _cachingServiceMock.Setup(x => x.AddAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<TimeSpan>(),
            cancellationToken))
            .Returns(Task.CompletedTask);

        // Act
        var (result, id) = await _authService.RegisterStudentAsync(dto, cancellationToken);

        // Assert
        result.Should().Be(UserOperationResult.Success);
        id.Should().Be(42);
        _tokenHelperMock.Verify(x => x.GenerateOTP(6), Times.Once);
        _cachingServiceMock.Verify(x => x.AddAsync(
            "user:email_confirmation:42",
            "123456",
            TimeSpan.FromMinutes(5),
            cancellationToken), Times.Once);
        _backgroundJobClientMock.Verify(x => x.Create(
            It.IsAny<Job>(),
            It.IsAny<IState>()), Times.Once);
    }

    [Theory]
    [InlineData(UserOperationResult.ValidationFailed)]
    [InlineData(UserOperationResult.EmailDuplicated)]
    [InlineData(UserOperationResult.UserCreationFailed)]
    [Trait("Category", TestCategories.Validation)]
    public async Task RegisterStudentAsync_UserCreationFails_ReturnsFailureWithZeroId(UserOperationResult userResult)
    {
        // Arrange
        var dto = new RegisterStudentDto
        {
            Name = "Test Student",
            Username = "teststudent",
            Email = "student@domain.com",
            Password = "password"
        };
        var cancellationToken = new CancellationToken();

        _userServiceMock.Setup(x => x.AddAsync(It.IsAny<AddUserDto>(), cancellationToken))
            .ReturnsAsync((userResult, 0));

        // Act
        var (result, id) = await _authService.RegisterStudentAsync(dto, cancellationToken);

        // Assert
        result.Should().Be(userResult);
        id.Should().Be(0);
        _studentServiceMock.Verify(x => x.AddAsync(It.IsAny<AddStudentDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(UserOperationResult.ValidationFailed)]
    [InlineData(UserOperationResult.InvalidUserId)]
    [InlineData(UserOperationResult.UserCreationFailed)]
    [Trait("Category", TestCategories.Validation)]
    public async Task RegisterStudentAsync_StudentCreationFails_ReturnsFailureWithZeroId(UserOperationResult studentResult)
    {
        // Arrange
        var dto = new RegisterStudentDto
        {
            Name = "Test Student",
            Username = "teststudent",
            Email = "student@domain.com",
            Password = "password"
        };
        var cancellationToken = new CancellationToken();

        _userServiceMock.Setup(x => x.AddAsync(It.IsAny<AddUserDto>(), cancellationToken))
            .ReturnsAsync((UserOperationResult.Success, 42));

        _studentServiceMock.Setup(x => x.AddAsync(It.IsAny<AddStudentDto>(), cancellationToken))
            .ReturnsAsync(studentResult);

        // Act
        var (result, id) = await _authService.RegisterStudentAsync(dto, cancellationToken);

        // Assert
        result.Should().Be(studentResult);
        id.Should().Be(0);
        _tokenHelperMock.Verify(x => x.GenerateOTP(6), Times.Never);
        _backgroundJobClientMock.Verify(x => x.Create(It.IsAny<Job>(), It.IsAny<IState>()), Times.Never);
    }

    #endregion

    #region Login Tests

    [Fact]
    [Trait("Category", TestCategories.Happy)]
    public async Task LoginAsync_ValidCredentials_ReturnsSuccessWithToken()
    {
        // Arrange
        var userId = 42;
        var loginDto = new UserLoginDto
        {
            Email = "test@domain.com",
            Password = "password"
        };
        var cancellationToken = new CancellationToken();

        var userInfo = new UserBasicInfo
        {
            ID = userId,
            Username = "testuser",
            Email = "test@domain.com",
            Name = "Test User",
            Role = UserRole.Instructor
        };

        _userServiceMock.Setup(x => x.VerifyUserPassword(loginDto, default))
            .ReturnsAsync((UserOperationResult.Success, userId));

        _userServiceMock.Setup(x => x.GetUserBasicInfoById(userId, default))
            .ReturnsAsync(userInfo);

        _tokenHelperMock.Setup(x => x.GenerateJWT(
            It.IsAny<UserTokenBaseClaims>(),
            It.IsAny<List<UserClaim>>()))
            .Returns("valid.jwt.token");

        // Act
        var (result, tokens) = await _authService.LoginAsync(loginDto, cancellationToken);

        // Assert
        result.Should().Be(UserOperationResult.Success);
        tokens.JwtToken.Should().Be("valid.jwt.token");
    }

    [Theory]
    [InlineData(UserOperationResult.InvalidCredentials)]
    [InlineData(UserOperationResult.UserNotFound)]
    [Trait("Category", TestCategories.Validation)]
    public async Task LoginAsync_InvalidCredentials_ReturnsFailureWithEmptyToken(UserOperationResult expectedResult)
    {
        // Arrange
        var loginDto = new UserLoginDto
        {
            Email = "test@domain.com",
            Password = "wrongpassword"
        };
        var cancellationToken = new CancellationToken();

        _userServiceMock.Setup(x => x.VerifyUserPassword(loginDto, default))
            .ReturnsAsync((expectedResult, default));

        // Act
        var (result, tokens) = await _authService.LoginAsync(loginDto, cancellationToken);

        // Assert
        result.Should().Be(expectedResult);
        tokens.JwtToken.Should().BeEmpty();
        _tokenHelperMock.Verify(x => x.GenerateJWT(
            It.IsAny<UserTokenBaseClaims>(),
            It.IsAny<List<UserClaim>>()), Times.Never);
    }

    [Fact]
    [Trait("Category", TestCategories.Business)]
    public async Task LoginAsync_TokenGenerationFails_ReturnsTokenGenerationFailed()
    {
        // Arrange
        var userId = 42;
        var loginDto = new UserLoginDto
        {
            Email = "test@domain.com",
            Password = "password"
        };
        var cancellationToken = new CancellationToken();

        var userInfo = new UserBasicInfo
        {
            ID = userId,
            Username = "testuser",
            Email = "test@domain.com",
            Name = "Test User",
            Role = UserRole.Instructor
        };

        _userServiceMock.Setup(x => x.VerifyUserPassword(loginDto, default))
            .ReturnsAsync((UserOperationResult.Success, userId));

        _userServiceMock.Setup(x => x.GetUserBasicInfoById(userId, default))
            .ReturnsAsync(userInfo);

        _tokenHelperMock.Setup(x => x.GenerateJWT(
            It.IsAny<UserTokenBaseClaims>(),
            It.IsAny<List<UserClaim>>()))
            .Returns(string.Empty);

        // Act
        var (result, tokens) = await _authService.LoginAsync(loginDto, cancellationToken);

        // Assert
        result.Should().Be(UserOperationResult.TokenGenerationFailed);
        tokens.JwtToken.Should().BeEmpty();
    }

    #endregion

    #region VerifyEmail Tests

    [Fact]
    [Trait("Category", TestCategories.Happy)]
    public async Task VerifyEmailAsync_ValidToken_ReturnsSuccess()
    {
        // Arrange
        var userId = 42;
        var token = "123456";
        var cancellationToken = new CancellationToken();

        _cachingServiceMock.Setup(x => x.GetAsync("user:email_confirmation:42", cancellationToken))
            .ReturnsAsync("123456");

        _userServiceMock.Setup(x => x.ConfirmUserEmail(userId, cancellationToken))
            .ReturnsAsync(UserEmailVerificationResult.Success);

        _cachingServiceMock.Setup(x => x.RemoveAsync("user:email_confirmation:42", cancellationToken))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _authService.VerifyEmailAsync(userId, token, cancellationToken);

        // Assert
        result.Should().Be(UserEmailVerificationResult.Success);
        _cachingServiceMock.Verify(x => x.RemoveAsync("user:email_confirmation:42", cancellationToken), Times.Once);
    }

    [Fact]
    [Trait("Category", TestCategories.Validation)]
    public async Task VerifyEmailAsync_ExpiredToken_ReturnsTokenExpired()
    {
        // Arrange
        var userId = 42;
        var token = "123456";
        var cancellationToken = new CancellationToken();

        _cachingServiceMock.Setup(x => x.GetAsync("user:email_confirmation:42", cancellationToken))
            .ReturnsAsync((string?)null);

        // Act
        var result = await _authService.VerifyEmailAsync(userId, token, cancellationToken);

        // Assert
        result.Should().Be(UserEmailVerificationResult.TokenExpired);
        _userServiceMock.Verify(x => x.ConfirmUserEmail(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    [Trait("Category", TestCategories.Validation)]
    public async Task VerifyEmailAsync_InvalidToken_ReturnsInvalidToken()
    {
        // Arrange
        var userId = 42;
        var token = "wrong-token";
        var cancellationToken = new CancellationToken();

        _cachingServiceMock.Setup(x => x.GetAsync("user:email_confirmation:42", cancellationToken))
            .ReturnsAsync("123456");

        // Act
        var result = await _authService.VerifyEmailAsync(userId, token, cancellationToken);

        // Assert
        result.Should().Be(UserEmailVerificationResult.InvalidToken);
        _userServiceMock.Verify(x => x.ConfirmUserEmail(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    [Trait("Category", TestCategories.Validation)]
    public async Task VerifyEmailAsync_UserServiceFails_ReturnsFailureResult()
    {
        // Arrange
        var userId = 42;
        var token = "123456";
        var cancellationToken = new CancellationToken();

        _cachingServiceMock.Setup(x => x.GetAsync("user:email_confirmation:42", cancellationToken))
            .ReturnsAsync("123456");

        _userServiceMock.Setup(x => x.ConfirmUserEmail(userId, cancellationToken))
            .ReturnsAsync(UserEmailVerificationResult.UserNotFound);

        // Act
        var result = await _authService.VerifyEmailAsync(userId, token, cancellationToken);

        // Assert
        result.Should().Be(UserEmailVerificationResult.UserNotFound);
        _cachingServiceMock.Verify(x => x.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region RefreshUserEmailVerificationToken Tests

    [Fact]
    [Trait("Category", TestCategories.Happy)]
    public async Task RefreshUserEmailVerificationToken_ValidUser_GeneratesNewTokenAndSendsEmail()
    {
        // Arrange
        var userId = 42;
        var cancellationToken = new CancellationToken();

        var userData = new UserBasicInfo
        {
            ID = userId,
            Name = "Test User",
            Email = "test@domain.com"
        };

        _cachingServiceMock.Setup(x => x.RemoveAsync("user:email_confirmation:42", cancellationToken))
            .Returns(Task.CompletedTask);

        _userServiceMock.Setup(x => x.GetUserBasicInfoById(userId, cancellationToken))
            .ReturnsAsync(userData);

        _tokenHelperMock.Setup(x => x.GenerateOTP(6)).Returns("654321");

        _cachingServiceMock.Setup(x => x.AddAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<TimeSpan>(),
            cancellationToken))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _authService.RefreshUserEmailVerificationToken(userId, cancellationToken);

        // Assert
        result.Should().Be(UserEmailVerificationResult.EmailJobSent);
        _cachingServiceMock.Verify(x => x.RemoveAsync("user:email_confirmation:42", cancellationToken), Times.Once);
        _tokenHelperMock.Verify(x => x.GenerateOTP(6), Times.Once);
        _cachingServiceMock.Verify(x => x.AddAsync(
            "user:email_confirmation:42",
            "654321",
            TimeSpan.FromMinutes(5),
            cancellationToken), Times.Once);
        _backgroundJobClientMock.Verify(x => x.Create(
            It.IsAny<Job>(),
            It.IsAny<IState>()), Times.Once);
    }

    [Fact]
    [Trait("Category", TestCategories.Validation)]
    public async Task RefreshUserEmailVerificationToken_UserNotFound_ReturnsUserNotFound()
    {
        // Arrange
        var userId = 42;
        var cancellationToken = new CancellationToken();

        _cachingServiceMock.Setup(x => x.RemoveAsync("user:email_confirmation:42", cancellationToken))
            .Returns(Task.CompletedTask);

        _userServiceMock.Setup(x => x.GetUserBasicInfoById(userId, cancellationToken))
            .ReturnsAsync((UserBasicInfo?)null);

        // Act
        var result = await _authService.RefreshUserEmailVerificationToken(userId, cancellationToken);

        // Assert
        result.Should().Be(UserEmailVerificationResult.UserNotFound);
        _tokenHelperMock.Verify(x => x.GenerateOTP(6), Times.Never);
        _backgroundJobClientMock.Verify(x => x.Create(It.IsAny<Job>(), It.IsAny<IState>()), Times.Never);
    }

    #endregion
}