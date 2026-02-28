using ExaminationSystem.Application.DTOs.StudentCourses;
using ExaminationSystem.Application.Services;
using ExaminationSystem.Domain.Entities;
using ExaminationSystem.Domain.Interfaces;
using FluentAssertions;
using MockQueryable;
using Moq;

namespace ExaminationSystem.UnitTests.Services;

public class StudentCourseServiceTests
{
    private readonly Mock<IRepository<Course>> _courseRepoMock;
    private readonly Mock<IRepository<StudentCourses>> _studentCoursesRepoMock;
    private readonly StudentCourseService _service;

    public StudentCourseServiceTests()
    {
        _courseRepoMock = new Mock<IRepository<Course>>();
        _studentCoursesRepoMock = new Mock<IRepository<StudentCourses>>();
        _service = new StudentCourseService(_courseRepoMock.Object, _studentCoursesRepoMock.Object);
    }

    #region EnrollInCourse Tests

    [Fact]
    [Trait("Category", TestCategories.Happy)]
    public async Task EnrollInCourse_ValidNewEnrollment_ReturnsSuccess()
    {
        // Arrange
        var dto = new StudentEnrollInCourseDto { CourseId = 1, StudentId = 10 };

        _courseRepoMock
            .Setup(x => x.CheckExistsByID(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _studentCoursesRepoMock
            .Setup(x => x.GetAll())
            .Returns(new List<StudentCourses>().AsQueryable().BuildMock());

        // Act
        var result = await _service.EnrollInCourse(dto);

        // Assert
        result.Should().Be(StudentCourseOperationResult.Success);
        _studentCoursesRepoMock.Verify(x => x.Add(It.IsAny<StudentCourses>(), It.IsAny<CancellationToken>()), Times.Once);
        _studentCoursesRepoMock.Verify(x => x.SaveChanges(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    [Trait("Category", TestCategories.Business)]
    public async Task EnrollInCourse_CourseNotFound_ReturnsCourseNotFound()
    {
        // Arrange
        var dto = new StudentEnrollInCourseDto { CourseId = 999, StudentId = 10 };

        _courseRepoMock
            .Setup(x => x.CheckExistsByID(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _service.EnrollInCourse(dto);

        // Assert
        result.Should().Be(StudentCourseOperationResult.CourseNotFound);
        _studentCoursesRepoMock.Verify(x => x.Add(It.IsAny<StudentCourses>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    [Trait("Category", TestCategories.Business)]
    public async Task EnrollInCourse_AlreadyEnrolled_ReturnsAlreadyEnrolled()
    {
        // Arrange
        var dto = new StudentEnrollInCourseDto { CourseId = 1, StudentId = 10 };

        _courseRepoMock
            .Setup(x => x.CheckExistsByID(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _studentCoursesRepoMock
            .Setup(x => x.GetAll())
            .Returns(new List<StudentCourses>
            {
                new StudentCourses { StudentID = 10, CourseID = 1, Student = null!, Course = null! }
            }.AsQueryable().BuildMock());

        // Act
        var result = await _service.EnrollInCourse(dto);

        // Assert
        result.Should().Be(StudentCourseOperationResult.AlreadyEnrolled);
    }

    [Fact]
    [Trait("Category", TestCategories.Business)]
    public async Task EnrollInCourse_MaxEnrollmentsExceeded_ReturnsMaxEnrollmentsExceeded()
    {
        // Arrange
        var dto = new StudentEnrollInCourseDto { CourseId = 99, StudentId = 10 };

        _courseRepoMock
            .Setup(x => x.CheckExistsByID(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // 5 existing enrollments = at limit
        _studentCoursesRepoMock
            .Setup(x => x.GetAll())
            .Returns(new List<StudentCourses>
            {
                new StudentCourses { StudentID = 10, CourseID = 1, Student = null!, Course = null! },
                new StudentCourses { StudentID = 10, CourseID = 2, Student = null!, Course = null! },
                new StudentCourses { StudentID = 10, CourseID = 3, Student = null!, Course = null! },
                new StudentCourses { StudentID = 10, CourseID = 4, Student = null!, Course = null! },
                new StudentCourses { StudentID = 10, CourseID = 5, Student = null!, Course = null! },
            }.AsQueryable().BuildMock());

        // Act
        var result = await _service.EnrollInCourse(dto);

        // Assert
        result.Should().Be(StudentCourseOperationResult.MaxEnrollmentsExceeded);
    }

    #endregion
}
