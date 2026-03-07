using ExaminationSystem.Application.DTOs.Instructor;
using ExaminationSystem.Application.Services;
using ExaminationSystem.Domain.Entities;
using ExaminationSystem.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace ExaminationSystem.UnitTests.Services;

public class InstructorServiceTests
{
    private readonly Mock<IRepository<Instructor>> _repositoryMock;
    private readonly Mock<ILogger<InstructorService>> _loggerMock;
    private readonly InstructorService _service;

    public InstructorServiceTests()
    {
        _repositoryMock = new Mock<IRepository<Instructor>>();
        _loggerMock = new Mock<ILogger<InstructorService>>();
        _service = new InstructorService(_repositoryMock.Object, _loggerMock.Object);
    }

    #region AddAsync Tests

    // Happy
    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(999)]
    [Trait("Category", TestCategories.Happy)]
    public async Task AddAsync_ValidAppUserId_ReturnsSuccess(int appUserId)
    {
        var dto = new AddInstructorDto { ID = appUserId };

        _repositoryMock
            .Setup(x => x.Add(It.IsAny<Instructor>(), It.IsAny<CancellationToken>()))
            .Callback<Instructor, CancellationToken>((i, _) => i.ID = 123);

        var result = await _service.AddAsync(dto);

        result.Should().Be(UserOperationResult.Success);
        _repositoryMock.Verify(x => x.Add(It.IsAny<Instructor>(), It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(x => x.SaveChanges(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    [Trait("Category", TestCategories.Happy)]
    public async Task AddAsync_WithCancellationToken_PassesTokenCorrectly()
    {
        var dto = new AddInstructorDto { ID = 1 };
        var cts = new CancellationTokenSource();

        _repositoryMock
            .Setup(x => x.Add(It.IsAny<Instructor>(), cts.Token))
            .Callback<Instructor, CancellationToken>((i, _) => i.ID = 456);

        var result = await _service.AddAsync(dto, cts.Token);

        result.Should().Be(UserOperationResult.Success);
        _repositoryMock.Verify(x => x.Add(It.IsAny<Instructor>(), cts.Token), Times.Once);
        _repositoryMock.Verify(x => x.SaveChanges(cts.Token), Times.Once);
    }

    [Theory]
    [InlineData(1, "Bio1", "Spec1")]
    [InlineData(2, "", "")]
    [InlineData(3, null, null)]
    [Trait("Category", TestCategories.Happy)]
    public async Task AddAsync_MapsDtoPropertiesToInstructor(int appUserId, string bio, string specialization)
    {
        var dto = new AddInstructorDto { ID = appUserId, Bio = bio, Specialization = specialization };
        Instructor? capturedInstructor = null;

        _repositoryMock
            .Setup(x => x.Add(It.IsAny<Instructor>(), It.IsAny<CancellationToken>()))
            .Callback<Instructor, CancellationToken>((i, _) => { i.ID = 321; capturedInstructor = i; });

        var result = await _service.AddAsync(dto);

        capturedInstructor?.Bio.Should().Be(bio);
        capturedInstructor?.Specialization.Should().Be(specialization);
        result.Should().Be(UserOperationResult.Success);
    }

    // Validation
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    [Trait("Category", TestCategories.Validation)]
    public async Task AddAsync_InvalidAppUserId_ReturnsInvalidUserId(int appUserId)
    {
        var dto = new AddInstructorDto { ID = appUserId };

        var result = await _service.AddAsync(dto);

        result.Should().Be(UserOperationResult.InvalidUserId);
        _repositoryMock.Verify(x => x.Add(It.IsAny<Instructor>(), It.IsAny<CancellationToken>()), Times.Never);
        _repositoryMock.Verify(x => x.SaveChanges(It.IsAny<CancellationToken>()), Times.Never);
    }

    // Infrastructure
    [Fact]
    [Trait("Category", TestCategories.Infrastructure)]
    public async Task AddAsync_RepositoryThrowsException_ReturnsUnknownError()
    {
        var dto = new AddInstructorDto { ID = 1 };
        _repositoryMock
            .Setup(x => x.Add(It.IsAny<Instructor>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("DB error"));

        await Assert.ThrowsAsync<Exception>(() => _service.AddAsync(dto));
    }

    #endregion
}