using ExaminationSystem.Application.DTOs.Courses;
using ExaminationSystem.Application.Services;
using ExaminationSystem.Domain.Entities;
using ExaminationSystem.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MockQueryable;
using Moq;
using System.Linq.Expressions;

namespace ExaminationSystem.UnitTests.Services;

public class CourseServiceTests
{
    private readonly Mock<IRepository<Course>> _courseRepoMock;
    private readonly Mock<ILogger<CourseService>> _loggerMock;
    private readonly CourseService _service;

    public CourseServiceTests()
    {
        _courseRepoMock = new Mock<IRepository<Course>>();
        _loggerMock = new Mock<ILogger<CourseService>>();
        _service = new CourseService(_courseRepoMock.Object, _loggerMock.Object);
    }

    #region GetAll Tests

    [Fact]
    [Trait("Category", TestCategories.Happy)]
    public async Task GetAll_ReturnsPaginatedAndSortedResults()
    {
        // Arrange
        var listDto = new ListCoursesDto
        {
            PageIndex = 0,
            PageSize = 10,
            OrderBy = nameof(Course.CreditHours),
            SortDirection = SortingDirection.Ascending,
            Title = "Math"
        };

        var courses = new List<Course>
        {
            new Course { ID = 1, Title = "Math 101", Description = "Intro", CreditHours = 3, InstructorID = 1, CreatedDate = DateTime.UtcNow },
            new Course { ID = 2, Title = "Advanced Math", Description = "Advanced", CreditHours = 4, InstructorID = 2, CreatedDate = DateTime.UtcNow }
        };

        _courseRepoMock
            .Setup(x => x.GetAll())
            .Returns(courses.AsQueryable().BuildMock());

        // Act
        var (data, totalCount) = await _service.GetAll(listDto);

        // Assert
        totalCount.Should().Be(2);
        data.Should().HaveCount(2);
        data.First().Title.Should().Be("Math 101"); // Due to ascending sort by CreditHours (3 vs 4)
    }

    [Fact]
    [Trait("Category", TestCategories.Happy)]
    public async Task GetAll_AppliesSearchFiltersCorrectly()
    {
        // Arrange
        var listDto = new ListCoursesDto
        {
            PageIndex = 0,
            PageSize = 10,
            CreditHours = 4,
            InstructorID = 2
        };

        var courses = new List<Course>
        {
            new Course { ID = 1, Title = "Math 101", Description = "Intro", CreditHours = 3, InstructorID = 1 },
            new Course { ID = 2, Title = "Physics 101", Description = "Intro", CreditHours = 4, InstructorID = 2 },
            new Course { ID = 3, Title = "Chemistry", Description = "Intro", CreditHours = 4, InstructorID = 1 }
        };

        _courseRepoMock
            .Setup(x => x.GetAll())
            .Returns(courses.AsQueryable().BuildMock());

        // Act
        var (data, totalCount) = await _service.GetAll(listDto);

        // Assert
        totalCount.Should().Be(1);
        data.Should().ContainSingle();
        data.First().ID.Should().Be(2);
    }

    #endregion

    #region Add Tests

    [Fact]
    [Trait("Category", TestCategories.Happy)]
    public async Task Add_ValidDto_ReturnsSuccessWithId()
    {
        // Arrange
        var dto = new AddCourseDto { Title = "Math 101", Description = "Intro to Math", CreditHours = 3, InstructorID = 1 };

        _courseRepoMock
            .Setup(x => x.GetByCondition(It.IsAny<Expression<Func<Course, bool>>>()))
            .Returns(new List<Course>().AsQueryable().BuildMock());

        _courseRepoMock
            .Setup(x => x.CheckExistsByCondition(It.IsAny<Expression<Func<Course, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _courseRepoMock
            .Setup(x => x.Add(It.IsAny<Course>(), It.IsAny<CancellationToken>()))
            .Callback<Course, CancellationToken>((c, _) => c.ID = 42);

        // Act
        var (result, id) = await _service.Add(dto);

        // Assert
        result.Should().Be(CourseOperationResult.Success);
        id.Should().Be(42);
        _courseRepoMock.Verify(x => x.SaveChanges(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("", "Desc", 3, 1)]
    [InlineData(null, "Desc", 3, 1)]
    [InlineData("Title", "", 3, 1)]
    [InlineData("Title", null, 3, 1)]
    [Trait("Category", TestCategories.Validation)]
    public async Task Add_EmptyTitleOrDescription_ReturnsValidationFailed(string title, string description, int credits, int instructorId)
    {
        // Arrange
        var dto = new AddCourseDto { Title = title, Description = description, CreditHours = credits, InstructorID = instructorId };

        // Act
        var (result, id) = await _service.Add(dto);

        // Assert
        result.Should().Be(CourseOperationResult.ValidationFailed);
        id.Should().Be(0);
        _courseRepoMock.Verify(x => x.Add(It.IsAny<Course>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [Trait("Category", TestCategories.Validation)]
    public async Task Add_InvalidCreditHours_ReturnsValidationFailed(int creditHours)
    {
        // Arrange
        var dto = new AddCourseDto { Title = "Math", Description = "Desc", CreditHours = creditHours, InstructorID = 1 };

        // Act
        var (result, id) = await _service.Add(dto);

        // Assert
        result.Should().Be(CourseOperationResult.ValidationFailed);
        id.Should().Be(0);
    }

    [Fact]
    [Trait("Category", TestCategories.Validation)]
    public async Task Add_InstructorIdZero_ReturnsValidationFailed()
    {
        // Arrange
        var dto = new AddCourseDto { Title = "Math", Description = "Desc", CreditHours = 3, InstructorID = 0 };

        // Act
        var (result, id) = await _service.Add(dto);

        // Assert
        result.Should().Be(CourseOperationResult.ValidationFailed);
        id.Should().Be(0);
    }

    [Fact]
    [Trait("Category", TestCategories.Business)]
    public async Task Add_InstructorExceededLimit_ReturnsMaxCoursesExceeded()
    {
        // Arrange
        var dto = new AddCourseDto { Title = "Math", Description = "Desc", CreditHours = 3, InstructorID = 1 };

        // 3 existing courses = at limit
        _courseRepoMock
            .Setup(x => x.GetByCondition(It.IsAny<Expression<Func<Course, bool>>>()))
            .Returns(new List<Course>
            {
                new Course { ID = 1, Instructor = null! },
                new Course { ID = 2, Instructor = null! },
                new Course { ID = 3, Instructor = null! }
            }.AsQueryable().BuildMock());

        // Act
        var (result, id) = await _service.Add(dto);

        // Assert
        result.Should().Be(CourseOperationResult.MaxCoursesExceeded);
        id.Should().Be(0);
    }

    [Fact]
    [Trait("Category", TestCategories.Business)]
    public async Task Add_DuplicateTitle_ReturnsDuplicateTitle()
    {
        // Arrange
        var dto = new AddCourseDto { Title = "Math 101", Description = "Desc", CreditHours = 3, InstructorID = 1 };

        _courseRepoMock
            .Setup(x => x.GetByCondition(It.IsAny<Expression<Func<Course, bool>>>()))
            .Returns(new List<Course>().AsQueryable().BuildMock());

        _courseRepoMock
            .Setup(x => x.CheckExistsByCondition(It.IsAny<Expression<Func<Course, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var (result, id) = await _service.Add(dto);

        // Assert
        result.Should().Be(CourseOperationResult.DuplicateTitle);
        id.Should().Be(0);
    }

    #endregion

    #region UpdateInfo Tests

    [Fact]
    [Trait("Category", TestCategories.Happy)]
    public async Task UpdateInfo_ExistsNoDuplicate_ReturnsSuccess()
    {
        // Arrange
        var dto = new UpdateCourseDto { ID = 1, Title = "Updated", Description = "Desc", CreditHours = 4 };

        _courseRepoMock
            .Setup(x => x.CheckExistsByID(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _courseRepoMock
            .Setup(x => x.CheckExistsByCondition(It.IsAny<Expression<Func<Course, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _service.UpdateInfo(dto);

        // Assert
        result.Should().Be(CourseOperationResult.Success);
        _courseRepoMock.Verify(x => x.SaveInclude(It.IsAny<Course>(), It.IsAny<string[]>()), Times.Once);
        _courseRepoMock.Verify(x => x.SaveChanges(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    [Trait("Category", TestCategories.Business)]
    public async Task UpdateInfo_NotFound_ReturnsNotFound()
    {
        // Arrange
        var dto = new UpdateCourseDto { ID = 999 };

        _courseRepoMock
            .Setup(x => x.CheckExistsByID(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _service.UpdateInfo(dto);

        // Assert
        result.Should().Be(CourseOperationResult.NotFound);
        _courseRepoMock.Verify(x => x.SaveInclude(It.IsAny<Course>(), It.IsAny<string[]>()), Times.Never);
    }

    [Fact]
    [Trait("Category", TestCategories.Business)]
    public async Task UpdateInfo_DuplicateTitle_ReturnsDuplicateTitle()
    {
        // Arrange
        var dto = new UpdateCourseDto { ID = 1, Title = "Existing Title" };

        _courseRepoMock
            .Setup(x => x.CheckExistsByID(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _courseRepoMock
            .Setup(x => x.CheckExistsByCondition(It.IsAny<Expression<Func<Course, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.UpdateInfo(dto);

        // Assert
        result.Should().Be(CourseOperationResult.DuplicateTitle);
    }

    #endregion

    #region Delete Tests

    [Fact]
    [Trait("Category", TestCategories.Happy)]
    public async Task Delete_OwnerExists_ReturnsSuccess()
    {
        // Arrange
        var dto = new DeleteCourseDto { CourseId = 1, ActorId = 10 };

        _courseRepoMock
            .Setup(x => x.GetByCondition(It.IsAny<Expression<Func<Course, bool>>>()))
            .Returns(new List<Course>
            {
                new Course { ID = 1, InstructorID = 10, Instructor = null! }
            }.AsQueryable().BuildMock());

        // Act
        var result = await _service.Delete(dto);

        // Assert
        result.Should().Be(CourseOperationResult.Success);
        _courseRepoMock.Verify(x => x.SoftDelete(It.IsAny<Course>()), Times.Once);
        _courseRepoMock.Verify(x => x.SaveChanges(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    [Trait("Category", TestCategories.Business)]
    public async Task Delete_NotFound_ReturnsNotFound()
    {
        // Arrange
        var dto = new DeleteCourseDto { CourseId = 999, ActorId = 10 };

        _courseRepoMock
            .Setup(x => x.GetByCondition(It.IsAny<Expression<Func<Course, bool>>>()))
            .Returns(new List<Course>().AsQueryable().BuildMock());

        // Act
        var result = await _service.Delete(dto);

        // Assert
        result.Should().Be(CourseOperationResult.NotFound);
        _courseRepoMock.Verify(x => x.SoftDelete(It.IsAny<Course>()), Times.Never);
    }

    [Fact]
    [Trait("Category", TestCategories.Business)]
    public async Task Delete_NotOwner_ReturnsNotOwner()
    {
        // Arrange
        var dto = new DeleteCourseDto { CourseId = 1, ActorId = 99 };

        _courseRepoMock
            .Setup(x => x.GetByCondition(It.IsAny<Expression<Func<Course, bool>>>()))
            .Returns(new List<Course>
            {
                new Course { ID = 1, InstructorID = 10, Instructor = null! }
            }.AsQueryable().BuildMock());

        // Act
        var result = await _service.Delete(dto);

        // Assert
        result.Should().Be(CourseOperationResult.NotOwner);
    }

    #endregion

    #region GetInstructorCoursesStats Tests

    [Fact]
    [Trait("Category", TestCategories.Happy)]
    public async Task GetInstructorCoursesStats_ReturnsAccurateCounts()
    {
        // Arrange
        int instructorId = 1;
        var courses = new List<Course>
        {
            new Course 
            { 
                ID = 1, 
                Title = "Course A", 
                InstructorID = instructorId,
                StudentCourses = new List<StudentCourses> { new StudentCourses(), new StudentCourses() },
                Exams = new List<Exam> { new Exam(), new Exam(), new Exam() }
            },
            new Course 
            { 
                ID = 2, 
                Title = "Course B", 
                InstructorID = instructorId,
                StudentCourses = new List<StudentCourses>(),
                Exams = new List<Exam> { new Exam() }
            },
            new Course 
            { 
                ID = 3, 
                Title = "Course C", 
                InstructorID = 99, // Different instructor
                StudentCourses = new List<StudentCourses>(),
                Exams = new List<Exam>()
            }
        };

        _courseRepoMock
            .Setup(x => x.GetAll())
            .Returns(courses.AsQueryable().BuildMock());

        // Act
        var result = await _service.GetInstructorCoursesStats(instructorId);

        // Assert
        result.Should().HaveCount(2);

        var courseAStats = result.First(c => c.CourseId == 1);
        courseAStats.CourseName.Should().Be("Course A");
        courseAStats.StudentCount.Should().Be(2);
        courseAStats.ExamsCount.Should().Be(3);

        var courseBStats = result.First(c => c.CourseId == 2);
        courseBStats.StudentCount.Should().Be(0);
        courseBStats.ExamsCount.Should().Be(1);
    }

    #endregion
}
