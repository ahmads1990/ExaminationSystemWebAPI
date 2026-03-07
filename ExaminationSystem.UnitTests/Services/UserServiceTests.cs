using ExaminationSystem.Application.DTOs.Users;
using ExaminationSystem.Application.InfraInterfaces;
using ExaminationSystem.Application.Interfaces;
using ExaminationSystem.Application.Services;
using ExaminationSystem.Domain.Entities;
using ExaminationSystem.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq.Expressions;

namespace ExaminationSystem.UnitTests.Services;

public class UserServiceTests
{
    private readonly Mock<IRepository<AppUser>> _userRepositoryMock;
    private readonly Mock<IPasswordHelper> _passwordHelperMock;
    private readonly Mock<ILogger<UserService>> _loggerMock;
    private readonly IUserService _userService;

    public UserServiceTests()
    {
        _userRepositoryMock = new Mock<IRepository<AppUser>>();
        _passwordHelperMock = new Mock<IPasswordHelper>();
        _loggerMock = new Mock<ILogger<UserService>>();
        _userService = new UserService(_userRepositoryMock.Object, _passwordHelperMock.Object, _loggerMock.Object);
    }

    #region AddAsync Tests

    // Happy
    [Fact]
    [Trait("Category", TestCategories.Happy)]
    public async Task AddAsync_ValidUser_ReturnsSuccess()
    {
        var dto = new AddUserDto
        {
            Name = "Test User",
            Username = "TestUser",
            Email = "Test@email.com",
            Password = "123456789"
        };

        _userRepositoryMock.Setup(x => x.CheckExistsByCondition(
            It.IsAny<Expression<Func<AppUser, bool>>>(),
            It.IsAny<CancellationToken>())
        ).ReturnsAsync(false);

        _passwordHelperMock.Setup(x => x.HashPassword(It.IsAny<string>()))
                           .Returns("hashed password");

        _userRepositoryMock.Setup(x => x.Add(It.IsAny<AppUser>(), It.IsAny<CancellationToken>()))
                           .Callback<AppUser, CancellationToken>((u, _) => u.ID = 123);

        _userRepositoryMock.Setup(x => x.SaveChanges(It.IsAny<CancellationToken>()));

        var result = await _userService.AddAsync(dto);

        result.Result.Should().Be(UserOperationResult.Success);
        result.Id.Should().Be(123);
        _userRepositoryMock.Verify(x => x.SaveChanges(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    [Trait("Category", TestCategories.Happy)]
    public async Task AddAsync_ValidUser_HashesPasswordCorrectly()
    {
        var dto = new AddUserDto
        {
            Name = "Test User",
            Username = "TestUser",
            Email = "test@email.com",
            Password = "original_password"
        };

        _userRepositoryMock.Setup(x => x.CheckExistsByCondition(
            It.IsAny<Expression<Func<AppUser, bool>>>(),
            It.IsAny<CancellationToken>())
        ).ReturnsAsync(false);

        _passwordHelperMock.Setup(x => x.HashPassword("original_password"))
                           .Returns("hashed_password");

        AppUser savedUser = default;
        _userRepositoryMock.Setup(x => x.Add(It.IsAny<AppUser>(), It.IsAny<CancellationToken>()))
                           .Callback<AppUser, CancellationToken>((user, _) => savedUser = user);

        _userRepositoryMock.Setup(x => x.SaveChanges(It.IsAny<CancellationToken>()))
                           .Callback<CancellationToken>(_ =>
                           {
                               if (savedUser != null)
                                   savedUser.ID = 1;
                           });

        var result = await _userService.AddAsync(dto);

        result.Result.Should().Be(UserOperationResult.Success);
        savedUser.Should().NotBeNull();
        savedUser.Password.Should().Be("hashed_password");
        _passwordHelperMock.Verify(x => x.HashPassword("original_password"), Times.Once);
    }

    // Validation
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [Trait("Category", TestCategories.Validation)]
    public async Task AddAsync_EmptyName_ReturnsValidationFailed(string name)
    {
        var dto = new AddUserDto
        {
            Name = name,
            Username = "TestUser",
            Email = "test@email.com",
            Password = "123456789"
        };

        var result = await _userService.AddAsync(dto);

        result.Result.Should().Be(UserOperationResult.ValidationFailed);
        result.Id.Should().Be(0);
        _userRepositoryMock.VerifyNoOtherCalls();
        _passwordHelperMock.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [Trait("Category", TestCategories.Validation)]
    public async Task AddAsync_EmptyUsername_ReturnsValidationFailed(string username)
    {
        var dto = new AddUserDto
        {
            Name = "Test User",
            Username = username,
            Email = "test@email.com",
            Password = "123456789"
        };

        var result = await _userService.AddAsync(dto);

        result.Result.Should().Be(UserOperationResult.ValidationFailed);
        result.Id.Should().Be(0);
        _userRepositoryMock.VerifyNoOtherCalls();
        _passwordHelperMock.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [Trait("Category", TestCategories.Validation)]
    public async Task AddAsync_EmptyEmail_ReturnsValidationFailed(string email)
    {
        var dto = new AddUserDto
        {
            Name = "Test User",
            Username = "TestUser",
            Email = email,
            Password = "123456789"
        };

        var result = await _userService.AddAsync(dto);

        result.Result.Should().Be(UserOperationResult.ValidationFailed);
        result.Id.Should().Be(0);
        _userRepositoryMock.VerifyNoOtherCalls();
        _passwordHelperMock.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [Trait("Category", TestCategories.Validation)]
    public async Task AddAsync_EmptyPassword_ReturnsValidationFailed(string password)
    {
        var dto = new AddUserDto
        {
            Name = "Test User",
            Username = "TestUser",
            Email = "test@email.com",
            Password = password
        };

        var result = await _userService.AddAsync(dto);

        result.Result.Should().Be(UserOperationResult.ValidationFailed);
        result.Id.Should().Be(0);
        _userRepositoryMock.VerifyNoOtherCalls();
        _passwordHelperMock.VerifyNoOtherCalls();
    }

    // Business
    [Fact]
    [Trait("Category", TestCategories.Business)]
    public async Task AddAsync_DuplicateEmail_ReturnsEmailDuplicated()
    {
        var dto = new AddUserDto
        {
            Name = "Test User",
            Username = "TestUser",
            Email = "existing@email.com",
            Password = "123456789"
        };

        _userRepositoryMock.Setup(x => x.CheckExistsByCondition(
            It.IsAny<Expression<Func<AppUser, bool>>>(),
            It.IsAny<CancellationToken>())
        ).ReturnsAsync(true);

        var result = await _userService.AddAsync(dto);

        result.Result.Should().Be(UserOperationResult.EmailDuplicated);
        result.Id.Should().Be(0);
        _userRepositoryMock.Verify(x => x.CheckExistsByCondition(
            It.IsAny<Expression<Func<AppUser, bool>>>(),
            It.IsAny<CancellationToken>()), Times.Once);
        _passwordHelperMock.VerifyNoOtherCalls();
    }

    // Infrastructure
    [Fact]
    [Trait("Category", TestCategories.Infrastructure)]
    public async Task AddAsync_RepositoryThrowsException_PropagatesException()
    {
        var dto = new AddUserDto
        {
            Name = "Test User",
            Username = "TestUser",
            Email = "test@email.com",
            Password = "123456789"
        };

        _userRepositoryMock.Setup(x => x.CheckExistsByCondition(
            It.IsAny<Expression<Func<AppUser, bool>>>(),
            It.IsAny<CancellationToken>())
        ).ThrowsAsync(new InvalidOperationException("Database error"));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _userService.AddAsync(dto));

        exception.Message.Should().Be("Database error");
    }

    #endregion
}