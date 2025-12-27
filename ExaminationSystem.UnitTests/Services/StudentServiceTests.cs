using ExaminationSystem.Application.DTOs.Student;
using ExaminationSystem.Application.Services;
using ExaminationSystem.Domain.Entities;
using ExaminationSystem.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace ExaminationSystem.UnitTests.Services;

public class StudentServiceTests
{
    private readonly Mock<IRepository<Student>> _repositoryMock;
    private readonly StudentService _service;

    public StudentServiceTests()
    {
        _repositoryMock = new Mock<IRepository<Student>>();
        _service = new StudentService(_repositoryMock.Object);
    }

    #region AddAsync Tests

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(999)]
    [Trait("Category", TestCategories.Happy)]
    public async Task AddAsync_ValidAppUserId_ReturnsSuccess(int appUserId)
    {
        var dto = new AddStudentDto { ID = appUserId };

        _repositoryMock
            .Setup(x => x.Add(It.IsAny<Student>(), It.IsAny<CancellationToken>()))
            .Callback<Student, CancellationToken>((s, _) => s.ID = 123);

        var result = await _service.AddAsync(dto);

        result.Should().Be(UserOperationResult.Success);
        _repositoryMock.Verify(x => x.Add(It.IsAny<Student>(), It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(x => x.SaveChanges(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    [Trait("Category", TestCategories.Happy)]
    public async Task AddAsync_WithCancellationToken_PassesTokenCorrectly()
    {
        var dto = new AddStudentDto { ID = 1 };
        var cts = new CancellationTokenSource();

        _repositoryMock
            .Setup(x => x.Add(It.IsAny<Student>(), cts.Token))
            .Callback<Student, CancellationToken>((s, _) => s.ID = 456);

        var result = await _service.AddAsync(dto, cts.Token);

        result.Should().Be(UserOperationResult.Success);
        _repositoryMock.Verify(x => x.Add(It.IsAny<Student>(), cts.Token), Times.Once);
        _repositoryMock.Verify(x => x.SaveChanges(cts.Token), Times.Once);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    [Trait("Category", TestCategories.Validation)]
    public async Task AddAsync_InvalidAppUserId_ReturnsInvalidUserId(int appUserId)
    {
        var dto = new AddStudentDto { ID = appUserId };

        var result = await _service.AddAsync(dto);

        result.Should().Be(UserOperationResult.InvalidUserId);
        _repositoryMock.Verify(x => x.Add(It.IsAny<Student>(), It.IsAny<CancellationToken>()), Times.Never);
        _repositoryMock.Verify(x => x.SaveChanges(It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion
}