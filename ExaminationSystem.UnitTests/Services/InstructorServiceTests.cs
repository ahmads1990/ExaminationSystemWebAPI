using ExaminationSystem.Application.DTOs.Instructor;
using ExaminationSystem.Application.Services;
using ExaminationSystem.Domain.Entities;
using ExaminationSystem.Domain.Interfaces;
using Moq;

namespace ExaminationSystem.UnitTests.Services;

/// <summary>
/// Unit tests for InstructorService
/// </summary>
public class InstructorServiceTests
{
    private readonly Mock<IRepository<Instructor>> _repositoryMock;
    private readonly InstructorService _service;

    public InstructorServiceTests()
    {
        _repositoryMock = new Mock<IRepository<Instructor>>();
        _service = new InstructorService(_repositoryMock.Object);
    }

    #region AddAsync Tests

    /// <summary>
    /// Tests AddAsync with valid AppUserId returns success
    /// </summary>
    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(999)]
    public async Task AddAsync_ValidAppUserId_ReturnsSuccess(int appUserId)
    {
        // Arrange
        var dto = new AddInstructorDto { AppUserId = appUserId };

        _repositoryMock
            .Setup(x => x.Add(It.IsAny<Instructor>(), It.IsAny<CancellationToken>()))
            .Callback<Instructor, CancellationToken>((i, _) => i.ID = 123);

        // Act
        var result = await _service.AddAsync(dto);

        // Assert
        Assert.Equal(AddInstructorResult.Success, result.result);
        Assert.Equal(123, result.Id);
        _repositoryMock.Verify(x => x.Add(It.IsAny<Instructor>(), It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(x => x.SaveChanges(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests AddAsync with invalid AppUserId returns InvalidUserId
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task AddAsync_InvalidAppUserId_ReturnsInvalidUserId(int appUserId)
    {
        // Arrange
        var dto = new AddInstructorDto { AppUserId = appUserId };

        // Act
        var result = await _service.AddAsync(dto);

        // Assert
        Assert.Equal(AddInstructorResult.InvalidUserId, result.result);
        Assert.Equal(0, result.Id);
        _repositoryMock.Verify(x => x.Add(It.IsAny<Instructor>(), It.IsAny<CancellationToken>()), Times.Never);
        _repositoryMock.Verify(x => x.SaveChanges(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Tests AddAsync with cancellation token
    /// </summary>
    [Fact]
    public async Task AddAsync_WithCancellationToken_PassesTokenCorrectly()
    {
        // Arrange
        var dto = new AddInstructorDto { AppUserId = 1 };
        var cts = new CancellationTokenSource();

        _repositoryMock
            .Setup(x => x.Add(It.IsAny<Instructor>(), cts.Token))
            .Callback<Instructor, CancellationToken>((i, _) => i.ID = 456);

        // Act
        var result = await _service.AddAsync(dto, cts.Token);

        // Assert
        Assert.Equal(AddInstructorResult.Success, result.result);
        _repositoryMock.Verify(x => x.Add(It.IsAny<Instructor>(), cts.Token), Times.Once);
        _repositoryMock.Verify(x => x.SaveChanges(cts.Token), Times.Once);
    }

    #endregion
}