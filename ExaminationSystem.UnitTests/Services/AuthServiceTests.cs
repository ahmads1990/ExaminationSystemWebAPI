using ExaminationSystem.Application.DTOs.Auth;
using ExaminationSystem.Application.DTOs.Instructor;
using ExaminationSystem.Application.DTOs.Users;
using ExaminationSystem.Application.InfraInterfaces;
using ExaminationSystem.Application.Interfaces;
using ExaminationSystem.Application.Services;
using FluentAssertions;
using Moq;

namespace ExaminationSystem.UnitTests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserService> _userServiceMock;
    private readonly Mock<IInstructorService> _instructorServiceMock;
    private readonly Mock<ITokenHelper> _tokenHelperMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _userServiceMock = new Mock<IUserService>();
        _instructorServiceMock = new Mock<IInstructorService>();
        _tokenHelperMock = new Mock<ITokenHelper>();
        _authService = new AuthService(_userServiceMock.Object, _instructorServiceMock.Object, _tokenHelperMock.Object);
    }

    #region RegisterInstructor Tests

    [Fact]
    [Trait("Category", TestCategories.Happy)]
    public async Task RegisterInstructorAsync_ValidData_ReturnsSuccessWithToken()
    {
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
            .ReturnsAsync((UserOperationResult.Success, 100));

        _tokenHelperMock.Setup(x => x.GenerateToken(
            It.IsAny<UserTokenBaseClaims>(),
            It.IsAny<List<UserClaim>>()))
            .Returns("valid.jwt.token");

        var result = await _authService.RegisterInstructorAsync(dto, cancellationToken);

        result.Result.Should().Be(UserOperationResult.Success);
        result.Token.Should().Be("valid.jwt.token");
    }

    [Theory]
    [InlineData(UserOperationResult.ValidationFailed)]
    [InlineData(UserOperationResult.EmailDuplicated)]
    [InlineData(UserOperationResult.UserCreationFailed)]
    [Trait("Category", TestCategories.Validation)]
    public async Task RegisterInstructorAsync_UserCreationFails_ReturnsUserCreationFailed(UserOperationResult userResult)
    {
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

        var result = await _authService.RegisterInstructorAsync(dto, cancellationToken);

        result.Result.Should().Be(UserOperationResult.UserCreationFailed);
        result.Token.Should().BeEmpty();
    }

    [Theory]
    [InlineData(UserOperationResult.ValidationFailed)]
    [InlineData(UserOperationResult.InvalidUserId)]
    [InlineData(UserOperationResult.UserCreationFailed)]
    [Trait("Category", TestCategories.Validation)]
    public async Task RegisterInstructorAsync_InstructorCreationFails_ReturnsUserCreationFailed(UserOperationResult instructorResult)
    {
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
            .ReturnsAsync((instructorResult, 0));

        var result = await _authService.RegisterInstructorAsync(dto, cancellationToken);

        result.Result.Should().Be(UserOperationResult.UserCreationFailed);
        result.Token.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", TestCategories.Business)]
    public async Task RegisterInstructorAsync_TokenGenerationFails_ReturnsTokenGenerationFailed()
    {
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
            .ReturnsAsync((UserOperationResult.Success, 100));

        _tokenHelperMock.Setup(x => x.GenerateToken(
            It.IsAny<UserTokenBaseClaims>(),
            It.IsAny<List<UserClaim>>()))
            .Returns(string.Empty);

        var result = await _authService.RegisterInstructorAsync(dto, cancellationToken);

        result.Result.Should().Be(UserOperationResult.TokenGenerationFailed);
        result.Token.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", TestCategories.Infrastructure)]
    public async Task RegisterInstructorAsync_UserServiceThrows_PropagatesException()
    {
        var dto = new RegisterInstructorDto
        {
            Name = "Test Instructor",
            Username = "testinstructor",
            Email = "test@domain.com",
            Password = "password"
        };
        var cancellationToken = new CancellationToken();

        _userServiceMock.Setup(x => x.AddAsync(It.IsAny<AddUserDto>(), cancellationToken))
            .ThrowsAsync(new Exception("User service error"));

        var act = async () => await _authService.RegisterInstructorAsync(dto, cancellationToken);

        await act.Should().ThrowAsync<Exception>().WithMessage("User service error");
    }

    [Fact]
    [Trait("Category", TestCategories.Infrastructure)]
    public async Task RegisterInstructorAsync_InstructorServiceThrows_PropagatesException()
    {
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
            .ThrowsAsync(new Exception("Instructor service error"));

        var act = async () => await _authService.RegisterInstructorAsync(dto, cancellationToken);

        await act.Should().ThrowAsync<Exception>().WithMessage("Instructor service error");
    }

    #endregion
}