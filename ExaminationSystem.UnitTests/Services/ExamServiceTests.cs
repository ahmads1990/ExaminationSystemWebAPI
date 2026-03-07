using ExaminationSystem.Application.DTOs.Exams;
using ExaminationSystem.Application.Services;
using ExaminationSystem.Domain.Entities;
using ExaminationSystem.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MockQueryable;
using Moq;
using System.Linq.Expressions;

namespace ExaminationSystem.UnitTests.Services;

public class ExamServiceTests
{
    private readonly Mock<IRepository<Exam>> _examRepoMock;
    private readonly Mock<IRepository<ExamQuestion>> _examQuestionRepoMock;
    private readonly Mock<IRepository<Question>> _questionRepoMock;
    private readonly Mock<ILogger<ExamService>> _loggerMock;
    private readonly ExamService _service;

    public ExamServiceTests()
    {
        _examRepoMock = new Mock<IRepository<Exam>>();
        _examQuestionRepoMock = new Mock<IRepository<ExamQuestion>>();
        _questionRepoMock = new Mock<IRepository<Question>>();
        _loggerMock = new Mock<ILogger<ExamService>>();
        _service = new ExamService(_examRepoMock.Object, _examQuestionRepoMock.Object, _questionRepoMock.Object, _loggerMock.Object);
    }

    #region GetAll Tests

    [Fact]
    [Trait("Category", TestCategories.Happy)]
    public async Task GetAll_ReturnsPaginatedAndFilteredResults()
    {
        // Arrange
        var listDto = new ListExamsDto
        {
            PageIndex = 0,
            PageSize = 10,
            Title = "Math",
            ExamType = ExamType.Quiz,
            OrderBy = nameof(Exam.DeadlineDate),
            SortDirection = SortingDirection.Descending
        };

        var exams = new List<Exam>
        {
            new Exam { ID = 1, Title = "Math Quiz 1", ExamType = ExamType.Quiz, DeadlineDate = DateTime.UtcNow.AddDays(1) },
            new Exam { ID = 2, Title = "Physics Quiz", ExamType = ExamType.Quiz, DeadlineDate = DateTime.UtcNow.AddDays(2) }
        };

        _examRepoMock
            .Setup(x => x.GetAll())
            .Returns(exams.AsQueryable().BuildMock());

        // Act
        var (data, totalCount) = await _service.GetAll(listDto);

        // Assert
        totalCount.Should().Be(1); // Only Math
        data.Should().HaveCount(1);
        data.First().Title.Should().Be("Math Quiz 1");
    }

    #endregion

    #region GetByID Tests

    [Fact]
    [Trait("Category", TestCategories.Happy)]
    public async Task GetByID_ExistingExam_ReturnsMappedDtoWithQuestions()
    {
        // Arrange
        var exam = new Exam
        {
            ID = 1,
            Title = "Test Exam",
            ExamType = ExamType.Quiz,
            ExamStatus = ExamStatus.Draft,
            TotalGrade = 50,
            ExamQuestions = new List<ExamQuestion>
            {
                new ExamQuestion { Exam = null!, Question = new Question { ID = 10, Body = "Q1", Score = 20 } },
                new ExamQuestion { Exam = null!, Question = new Question { ID = 11, Body = "Q2", Score = 30 } }
            }
        };

        _examRepoMock
            .Setup(x => x.GetByID(1))
            .Returns(new List<Exam> { exam }.AsQueryable().BuildMock());

        // Act
        var result = await _service.GetByID(1);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be("Test Exam");
        result.TotalGrade.Should().Be(50);
    }

    [Fact]
    [Trait("Category", TestCategories.Business)]
    public async Task GetByID_NonExistingExam_ReturnsNull()
    {
        // Arrange
        _examRepoMock
            .Setup(x => x.GetByID(999))
            .Returns(new List<Exam>().AsQueryable().BuildMock());

        // Act
        var result = await _service.GetByID(999);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetExamSubmissions Tests

    [Fact]
    [Trait("Category", TestCategories.Happy)]
    public async Task GetExamSubmissions_InstructorOwnsExam_ReturnsAttempts()
    {
        // Arrange
        int examId = 1, instructorId = 5;

        var exam = new Exam
        {
            ID = examId,
            Title = "Test Exam",
            Course = new Course { InstructorID = instructorId, Title = "Test Course" },
            ExamAttempts = new List<ExamAttempt>
            {
                new ExamAttempt
                {
                    ID = 1,
                    StudentId = 100,
                    Score = 90,
                    ExamAttemptStatus = ExamAttemptStatus.Completed
                }
            }
        };

        _examRepoMock
            .Setup(x => x.GetByCondition(It.IsAny<Expression<Func<Exam, bool>>>()))
            .Returns(new List<Exam> { exam }.AsQueryable().BuildMock());

        // Act
        var (result, attempts) = await _service.GetExamSubmissions(examId, instructorId);

        // Assert
        result.Should().Be(ExamOperationResult.Success);
        attempts.Should().HaveCount(1);
        attempts!.First().ExamTitle.Should().Be("Test Exam");
    }

    [Fact]
    [Trait("Category", TestCategories.Business)]
    public async Task GetExamSubmissions_ExamNotFound_ReturnsNotFound()
    {
        // Arrange
        _examRepoMock
            .Setup(x => x.GetByCondition(It.IsAny<Expression<Func<Exam, bool>>>()))
            .Returns(new List<Exam>().AsQueryable().BuildMock());

        // Act
        var (result, attempts) = await _service.GetExamSubmissions(999, 5);

        // Assert
        result.Should().Be(ExamOperationResult.NotFound);
        attempts.Should().BeNull();
    }

    [Fact]
    [Trait("Category", TestCategories.Business)]
    public async Task GetExamSubmissions_NotOwner_ReturnsNotOwner()
    {
        // Arrange
        int examId = 1, instructorId = 5, wrongInstructorId = 99;

        var exam = new Exam
        {
            ID = examId,
            Course = new Course { InstructorID = instructorId } // Actual owner
        };

        _examRepoMock
            .Setup(x => x.GetByCondition(It.IsAny<Expression<Func<Exam, bool>>>()))
            .Returns(new List<Exam> { exam }.AsQueryable().BuildMock());

        // Act
        var (result, attempts) = await _service.GetExamSubmissions(examId, wrongInstructorId);

        // Assert
        result.Should().Be(ExamOperationResult.NotOwner);
        attempts.Should().BeNull();
    }

    #endregion

    #region Add Tests

    [Fact]
    [Trait("Category", TestCategories.Happy)]
    public async Task Add_ValidDto_ReturnsSuccessWithId()
    {
        // Arrange
        var dto = new AddExamDto { ExamType = ExamType.Quiz, CourseID = 1 };

        _examRepoMock
            .Setup(x => x.Add(It.IsAny<Exam>(), It.IsAny<CancellationToken>()))
            .Callback<Exam, CancellationToken>((e, _) => e.ID = 42);

        // Act
        var (result, id) = await _service.Add(dto);

        // Assert
        result.Should().Be(ExamOperationResult.Success);
        id.Should().Be(42);
        _examRepoMock.Verify(x => x.SaveChanges(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    [Trait("Category", TestCategories.Happy)]
    public async Task Add_ValidDto_CallsRepositoryAddAndSave()
    {
        // Arrange
        var dto = new AddExamDto { ExamType = ExamType.Final, CourseID = 5 };

        // Act
        await _service.Add(dto);

        // Assert
        _examRepoMock.Verify(x => x.Add(It.IsAny<Exam>(), It.IsAny<CancellationToken>()), Times.Once);
        _examRepoMock.Verify(x => x.SaveChanges(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Update Tests

    [Fact]
    [Trait("Category", TestCategories.Happy)]
    public async Task Update_ExamExists_ReturnsSuccess()
    {
        // Arrange
        var dto = new UpdateExamDto { ID = 1, ExamType = ExamType.Quiz };
        var exam = new Exam { ID = 1, Course = null! };

        _examRepoMock
            .Setup(x => x.GetByID(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(exam);

        // Act
        var result = await _service.Update(dto);

        // Assert
        result.Should().Be(ExamOperationResult.Success);
        _examRepoMock.Verify(x => x.Update(It.IsAny<Exam>()), Times.Once);
        _examRepoMock.Verify(x => x.SaveChanges(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    [Trait("Category", TestCategories.Business)]
    public async Task Update_ExamNotFound_ReturnsNotFound()
    {
        // Arrange
        var dto = new UpdateExamDto { ID = 999 };

        _examRepoMock
            .Setup(x => x.GetByID(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Exam?)null);

        // Act
        var result = await _service.Update(dto);

        // Assert
        result.Should().Be(ExamOperationResult.NotFound);
        _examRepoMock.Verify(x => x.Update(It.IsAny<Exam>()), Times.Never);
        _examRepoMock.Verify(x => x.SaveChanges(It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Publish Tests

    [Fact]
    [Trait("Category", TestCategories.Happy)]
    public async Task Publish_DraftExamWithQuestions_ReturnsSuccess()
    {
        // Arrange
        var dto = new PublishExamDto { ID = 1, PublishDate = new DateTime(2026, 6, 1) };

        _examRepoMock
            .Setup(x => x.GetByCondition(It.IsAny<Expression<Func<Exam, bool>>>()))
            .Returns(new List<Exam>
            {
                new Exam
                {
                    ID = 1,
                    ExamStatus = ExamStatus.Draft,
                    TotalGrade = 10,
                    ExamQuestions = new List<ExamQuestion>
                    {
                        new ExamQuestion { Exam = null!, Question = new Question { Score = 10 } }
                    },
                    Course = null!
                }
            }.AsQueryable().BuildMock());

        // Act
        var result = await _service.Publish(dto);

        // Assert
        result.Should().Be(ExamOperationResult.Success);
        _examRepoMock.Verify(x => x.SaveInclude(It.IsAny<Exam>(), It.Is<string[]>(p => p.Contains(nameof(Exam.ExamStatus)))), Times.Once);
        _examRepoMock.Verify(x => x.SaveChanges(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    [Trait("Category", TestCategories.Happy)]
    public async Task Publish_NullPublishDate_DefaultsToUtcNow()
    {
        // Arrange
        var dto = new PublishExamDto { ID = 1, PublishDate = null };

        _examRepoMock
            .Setup(x => x.GetByCondition(It.IsAny<Expression<Func<Exam, bool>>>()))
            .Returns(new List<Exam>
            {
                new Exam
                {
                    ID = 1,
                    ExamStatus = ExamStatus.Draft,
                    TotalGrade = 10,
                    ExamQuestions = new List<ExamQuestion>
                    {
                        new ExamQuestion { Exam = null!, Question = new Question { Score = 10 } }
                    },
                    Course = null!
                }
            }.AsQueryable().BuildMock());

        Exam? capturedExam = null;
        _examRepoMock
            .Setup(x => x.SaveInclude(It.IsAny<Exam>(), It.IsAny<string[]>()))
            .Callback<Exam, string[]>((e, _) => capturedExam = e);

        // Act
        var before = DateTime.UtcNow;
        var result = await _service.Publish(dto);
        var after = DateTime.UtcNow;

        // Assert
        result.Should().Be(ExamOperationResult.Success);
        capturedExam.Should().NotBeNull();
        capturedExam!.PublishDate.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    [Trait("Category", TestCategories.Business)]
    public async Task Publish_ExamNotFound_ReturnsNotFound()
    {
        // Arrange
        var dto = new PublishExamDto { ID = 999 };

        _examRepoMock
            .Setup(x => x.GetByCondition(It.IsAny<Expression<Func<Exam, bool>>>()))
            .Returns(new List<Exam>().AsQueryable().BuildMock());

        // Act
        var result = await _service.Publish(dto);

        // Assert
        result.Should().Be(ExamOperationResult.NotFound);
        _examRepoMock.Verify(x => x.SaveInclude(It.IsAny<Exam>(), It.IsAny<string[]>()), Times.Never);
    }

    [Fact]
    [Trait("Category", TestCategories.Business)]
    public async Task Publish_AlreadyPublished_ReturnsAlreadyPublished()
    {
        // Arrange
        var dto = new PublishExamDto { ID = 1 };

        _examRepoMock
            .Setup(x => x.GetByCondition(It.IsAny<Expression<Func<Exam, bool>>>()))
            .Returns(new List<Exam>
            {
                new Exam
                {
                    ID = 1,
                    ExamStatus = ExamStatus.Published,
                    TotalGrade = 10,
                    ExamQuestions = new List<ExamQuestion>
                    {
                        new ExamQuestion { Exam = null!, Question = new Question { Score = 10 } }
                    },
                    Course = null!
                }
            }.AsQueryable().BuildMock());

        // Act
        var result = await _service.Publish(dto);

        // Assert
        result.Should().Be(ExamOperationResult.AlreadyPublished);
    }

    [Fact]
    [Trait("Category", TestCategories.Business)]
    public async Task Publish_ArchivedExam_ReturnsExamArchived()
    {
        // Arrange
        var dto = new PublishExamDto { ID = 1 };

        _examRepoMock
            .Setup(x => x.GetByCondition(It.IsAny<Expression<Func<Exam, bool>>>()))
            .Returns(new List<Exam>
            {
                new Exam { ID = 1, ExamStatus = ExamStatus.Archived, Course = null! }
            }.AsQueryable().BuildMock());

        // Act
        var result = await _service.Publish(dto);

        // Assert
        result.Should().Be(ExamOperationResult.ExamArchived);
    }

    [Fact]
    [Trait("Category", TestCategories.Business)]
    public async Task Publish_NoQuestions_ReturnsNoQuestions()
    {
        // Arrange
        var dto = new PublishExamDto { ID = 1 };

        _examRepoMock
            .Setup(x => x.GetByCondition(It.IsAny<Expression<Func<Exam, bool>>>()))
            .Returns(new List<Exam>
            {
                new Exam { ID = 1, ExamStatus = ExamStatus.Draft, TotalGrade = 10, ExamQuestions = new List<ExamQuestion>(), Course = null! }
            }.AsQueryable().BuildMock());

        // Act
        var result = await _service.Publish(dto);

        // Assert
        result.Should().Be(ExamOperationResult.NoQuestions);
    }

    [Fact]
    [Trait("Category", TestCategories.Business)]
    public async Task Publish_QuestionScoresMismatch_ReturnsScoresMismatch()
    {
        // Arrange
        var dto = new PublishExamDto { ID = 1 };

        _examRepoMock
            .Setup(x => x.GetByCondition(It.IsAny<Expression<Func<Exam, bool>>>()))
            .Returns(new List<Exam>
            {
                new Exam
                {
                    ID = 1,
                    ExamStatus = ExamStatus.Draft,
                    TotalGrade = 100,       // expects 100 but questions only sum to 30
                    ExamQuestions = new List<ExamQuestion>
                    {
                        new ExamQuestion { Exam = null!, Question = new Question { Score = 10 } },
                        new ExamQuestion { Exam = null!, Question = new Question { Score = 20 } }
                    },
                    Course = null!
                }
            }.AsQueryable().BuildMock());

        // Act
        var result = await _service.Publish(dto);

        // Assert
        result.Should().Be(ExamOperationResult.ScoresMismatch);
        _examRepoMock.Verify(x => x.SaveInclude(It.IsAny<Exam>(), It.IsAny<string[]>()), Times.Never);
    }

    #endregion

    #region UnPublish Tests

    [Fact]
    [Trait("Category", TestCategories.Happy)]
    public async Task UnPublish_PublishedExamNoSubmissions_ReturnsSuccess()
    {
        // Arrange
        _examRepoMock
            .Setup(x => x.GetByCondition(It.IsAny<Expression<Func<Exam, bool>>>()))
            .Returns(new List<Exam>
            {
                new Exam { ID = 1, ExamStatus = ExamStatus.Published, ExamAttempts = new List<ExamAttempt>(), Course = null! }
            }.AsQueryable().BuildMock());

        // Act
        var result = await _service.UnPublish(1);

        // Assert
        result.Should().Be(ExamOperationResult.Success);
        _examRepoMock.Verify(x => x.SaveInclude(It.IsAny<Exam>(), It.Is<string[]>(p => p.Contains(nameof(Exam.ExamStatus)))), Times.Once);
    }

    [Fact]
    [Trait("Category", TestCategories.Business)]
    public async Task UnPublish_ExamNotFound_ReturnsNotFound()
    {
        // Arrange
        _examRepoMock
            .Setup(x => x.GetByCondition(It.IsAny<Expression<Func<Exam, bool>>>()))
            .Returns(new List<Exam>().AsQueryable().BuildMock());

        // Act
        var result = await _service.UnPublish(999);

        // Assert
        result.Should().Be(ExamOperationResult.NotFound);
    }

    [Fact]
    [Trait("Category", TestCategories.Business)]
    public async Task UnPublish_ArchivedExam_ReturnsExamArchived()
    {
        // Arrange
        _examRepoMock
            .Setup(x => x.GetByCondition(It.IsAny<Expression<Func<Exam, bool>>>()))
            .Returns(new List<Exam>
            {
                new Exam { ID = 1, ExamStatus = ExamStatus.Archived, Course = null! }
            }.AsQueryable().BuildMock());

        // Act
        var result = await _service.UnPublish(1);

        // Assert
        result.Should().Be(ExamOperationResult.ExamArchived);
    }

    [Fact]
    [Trait("Category", TestCategories.Business)]
    public async Task UnPublish_AlreadyUnpublished_ReturnsAlreadyUnpublished()
    {
        // Arrange
        _examRepoMock
            .Setup(x => x.GetByCondition(It.IsAny<Expression<Func<Exam, bool>>>()))
            .Returns(new List<Exam>
            {
                new Exam { ID = 1, ExamStatus = ExamStatus.Draft, ExamAttempts = new List<ExamAttempt>(), Course = null! }
            }.AsQueryable().BuildMock());

        // Act
        var result = await _service.UnPublish(1);

        // Assert
        result.Should().Be(ExamOperationResult.AlreadyUnpublished);
    }

    [Fact]
    [Trait("Category", TestCategories.Business)]
    public async Task UnPublish_HasSubmissions_ReturnsHasSubmissions()
    {
        // Arrange
        _examRepoMock
            .Setup(x => x.GetByCondition(It.IsAny<Expression<Func<Exam, bool>>>()))
            .Returns(new List<Exam>
            {
                new Exam { ID = 1, ExamStatus = ExamStatus.Published, ExamAttempts = new List<ExamAttempt> { new ExamAttempt { Exam = null!, Student = null! } }, Course = null! }
            }.AsQueryable().BuildMock());

        // Act
        var result = await _service.UnPublish(1);

        // Assert
        result.Should().Be(ExamOperationResult.HasSubmissions);
    }

    #endregion

    #region AssignQuestions Tests

    [Fact]
    [Trait("Category", TestCategories.Happy)]
    public async Task AssignQuestions_AllValid_AssignsAllReturnsEmptyRejected()
    {
        // Arrange
        var dto = new AssignQuestionsDto { ExamId = 1, QuestionIds = new List<int> { 10, 20 } };

        _examRepoMock
            .Setup(x => x.GetByCondition(It.IsAny<Expression<Func<Exam, bool>>>()))
            .Returns(new List<Exam>
            {
                new Exam { ID = 1, ExamStatus = ExamStatus.Draft, Course = null! }
            }.AsQueryable().BuildMock());

        _examQuestionRepoMock
            .Setup(x => x.GetByCondition(It.IsAny<Expression<Func<ExamQuestion, bool>>>()))
            .Returns(new List<ExamQuestion>().AsQueryable().BuildMock());

        _questionRepoMock
            .Setup(x => x.GetByCondition(It.IsAny<Expression<Func<Question, bool>>>()))
            .Returns(new List<Question>
            {
                new Question { ID = 10, Body = "Q1" },
                new Question { ID = 20, Body = "Q2" }
            }.AsQueryable().BuildMock());

        // Act
        var (result, rejected) = await _service.AssignQuestions(dto);

        // Assert
        result.Should().Be(ExamOperationResult.Success);
        rejected.Should().BeEmpty();
        _examQuestionRepoMock.Verify(x => x.AddRange(It.Is<IEnumerable<ExamQuestion>>(eq => eq.Count() == 2), It.IsAny<CancellationToken>()), Times.Once);
        _examQuestionRepoMock.Verify(x => x.SaveChanges(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    [Trait("Category", TestCategories.Happy)]
    public async Task AssignQuestions_MixedValid_AssignsNewRejectsAlreadyAssigned()
    {
        // Arrange
        var dto = new AssignQuestionsDto { ExamId = 1, QuestionIds = new List<int> { 10, 20 } };

        _examRepoMock
            .Setup(x => x.GetByCondition(It.IsAny<Expression<Func<Exam, bool>>>()))
            .Returns(new List<Exam>
            {
                new Exam { ID = 1, ExamStatus = ExamStatus.Draft, Course = null! }
            }.AsQueryable().BuildMock());

        _examQuestionRepoMock
            .Setup(x => x.GetByCondition(It.IsAny<Expression<Func<ExamQuestion, bool>>>()))
            .Returns(new List<ExamQuestion>
            {
                new ExamQuestion { ExamId = 1, QuestionId = 10, Exam = null!, Question = null! }
            }.AsQueryable().BuildMock());

        _questionRepoMock
            .Setup(x => x.GetByCondition(It.IsAny<Expression<Func<Question, bool>>>()))
            .Returns(new List<Question>
            {
                new Question { ID = 10, Body = "Q1" },
                new Question { ID = 20, Body = "Q2" }
            }.AsQueryable().BuildMock());

        // Act
        var (result, rejected) = await _service.AssignQuestions(dto);

        // Assert
        result.Should().Be(ExamOperationResult.Success);
        rejected.Should().HaveCount(1);
        rejected.First().Id.Should().Be(10);
        rejected.First().Reason.Should().Be(RejectionReason.AlreadyAssigned);
        _examQuestionRepoMock.Verify(x => x.AddRange(It.Is<IEnumerable<ExamQuestion>>(eq => eq.Count() == 1), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    [Trait("Category", TestCategories.Business)]
    public async Task AssignQuestions_ExamNotFound_ReturnsNotFound()
    {
        // Arrange
        var dto = new AssignQuestionsDto { ExamId = 999, QuestionIds = new List<int> { 10 } };

        _examRepoMock
            .Setup(x => x.GetByCondition(It.IsAny<Expression<Func<Exam, bool>>>()))
            .Returns(new List<Exam>().AsQueryable().BuildMock());

        // Act
        var (result, rejected) = await _service.AssignQuestions(dto);

        // Assert
        result.Should().Be(ExamOperationResult.NotFound);
        rejected.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", TestCategories.Business)]
    public async Task AssignQuestions_ArchivedExam_ReturnsExamArchived()
    {
        // Arrange
        var dto = new AssignQuestionsDto { ExamId = 1, QuestionIds = new List<int> { 10 } };

        _examRepoMock
            .Setup(x => x.GetByCondition(It.IsAny<Expression<Func<Exam, bool>>>()))
            .Returns(new List<Exam>
            {
                new Exam { ID = 1, ExamStatus = ExamStatus.Archived, Course = null! }
            }.AsQueryable().BuildMock());

        // Act
        var (result, rejected) = await _service.AssignQuestions(dto);

        // Assert
        result.Should().Be(ExamOperationResult.ExamArchived);
        rejected.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", TestCategories.Business)]
    public async Task AssignQuestions_PublishedExam_ReturnsExamPublished()
    {
        // Arrange
        var dto = new AssignQuestionsDto { ExamId = 1, QuestionIds = new List<int> { 10 } };

        _examRepoMock
            .Setup(x => x.GetByCondition(It.IsAny<Expression<Func<Exam, bool>>>()))
            .Returns(new List<Exam>
            {
                new Exam { ID = 1, ExamStatus = ExamStatus.Published, Course = null! }
            }.AsQueryable().BuildMock());

        // Act
        var (result, rejected) = await _service.AssignQuestions(dto);

        // Assert
        result.Should().Be(ExamOperationResult.ExamPublished);
        rejected.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", TestCategories.Business)]
    public async Task AssignQuestions_SomeQuestionsNotFound_RejectsWithNotFound()
    {
        // Arrange
        var dto = new AssignQuestionsDto { ExamId = 1, QuestionIds = new List<int> { 10, 99 } };

        _examRepoMock
            .Setup(x => x.GetByCondition(It.IsAny<Expression<Func<Exam, bool>>>()))
            .Returns(new List<Exam>
            {
                new Exam { ID = 1, ExamStatus = ExamStatus.Draft, Course = null! }
            }.AsQueryable().BuildMock());

        _examQuestionRepoMock
            .Setup(x => x.GetByCondition(It.IsAny<Expression<Func<ExamQuestion, bool>>>()))
            .Returns(new List<ExamQuestion>().AsQueryable().BuildMock());

        _questionRepoMock
            .Setup(x => x.GetByCondition(It.IsAny<Expression<Func<Question, bool>>>()))
            .Returns(new List<Question>
            {
                new Question { ID = 10, Body = "Q1" }
            }.AsQueryable().BuildMock());

        // Act
        var (result, rejected) = await _service.AssignQuestions(dto);

        // Assert
        result.Should().Be(ExamOperationResult.Success);
        rejected.Should().HaveCount(1);
        rejected.First().Id.Should().Be(99);
        rejected.First().Reason.Should().Be(RejectionReason.NotFound);
    }

    [Fact]
    [Trait("Category", TestCategories.Business)]
    public async Task AssignQuestions_AllAlreadyAssigned_ReturnsSuccessWithFullRejectedList()
    {
        // Arrange
        var dto = new AssignQuestionsDto { ExamId = 1, QuestionIds = new List<int> { 10 } };

        _examRepoMock
            .Setup(x => x.GetByCondition(It.IsAny<Expression<Func<Exam, bool>>>()))
            .Returns(new List<Exam>
            {
                new Exam { ID = 1, ExamStatus = ExamStatus.Draft, Course = null! }
            }.AsQueryable().BuildMock());

        _examQuestionRepoMock
            .Setup(x => x.GetByCondition(It.IsAny<Expression<Func<ExamQuestion, bool>>>()))
            .Returns(new List<ExamQuestion>
            {
                new ExamQuestion { ExamId = 1, QuestionId = 10, Exam = null!, Question = null! }
            }.AsQueryable().BuildMock());

        _questionRepoMock
            .Setup(x => x.GetByCondition(It.IsAny<Expression<Func<Question, bool>>>()))
            .Returns(new List<Question>
            {
                new Question { ID = 10, Body = "Q1" }
            }.AsQueryable().BuildMock());

        // Act
        var (result, rejected) = await _service.AssignQuestions(dto);

        // Assert
        result.Should().Be(ExamOperationResult.Success);
        rejected.Should().HaveCount(1);
        rejected.First().Reason.Should().Be(RejectionReason.AlreadyAssigned);
        _examQuestionRepoMock.Verify(x => x.AddRange(It.IsAny<IEnumerable<ExamQuestion>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region UnassignQuestions Tests

    [Fact]
    [Trait("Category", TestCategories.Happy)]
    public async Task UnassignQuestions_AllAssigned_UnassignsAllReturnsEmptyRejected()
    {
        // Arrange
        var dto = new AssignQuestionsDto { ExamId = 1, QuestionIds = new List<int> { 10, 20 } };

        _examRepoMock
            .Setup(x => x.GetByCondition(It.IsAny<Expression<Func<Exam, bool>>>()))
            .Returns(new List<Exam>
            {
                new Exam { ID = 1, ExamStatus = ExamStatus.Draft, ExamAttempts = new List<ExamAttempt>(), Course = null! }
            }.AsQueryable().BuildMock());

        _examQuestionRepoMock
            .Setup(x => x.GetByCondition(It.IsAny<Expression<Func<ExamQuestion, bool>>>()))
            .Returns(new List<ExamQuestion>
            {
                new ExamQuestion { ExamId = 1, QuestionId = 10, Exam = null!, Question = null! },
                new ExamQuestion { ExamId = 1, QuestionId = 20, Exam = null!, Question = null! }
            }.AsQueryable().BuildMock());

        // Act
        var (result, rejected) = await _service.UnassignQuestions(dto);

        // Assert
        result.Should().Be(ExamOperationResult.Success);
        rejected.Should().BeEmpty();
        _examQuestionRepoMock.Verify(x => x.DeleteRange(It.Is<IEnumerable<ExamQuestion>>(eq => eq.Count() == 2)), Times.Once);
    }

    [Fact]
    [Trait("Category", TestCategories.Happy)]
    public async Task UnassignQuestions_MixedValid_UnassignsValidRejectsNotAssigned()
    {
        // Arrange
        var dto = new AssignQuestionsDto { ExamId = 1, QuestionIds = new List<int> { 10, 30 } };

        _examRepoMock
            .Setup(x => x.GetByCondition(It.IsAny<Expression<Func<Exam, bool>>>()))
            .Returns(new List<Exam>
            {
                new Exam { ID = 1, ExamStatus = ExamStatus.Draft, ExamAttempts = new List<ExamAttempt>(), Course = null! }
            }.AsQueryable().BuildMock());

        _examQuestionRepoMock
            .Setup(x => x.GetByCondition(It.IsAny<Expression<Func<ExamQuestion, bool>>>()))
            .Returns(new List<ExamQuestion>
            {
                new ExamQuestion { ExamId = 1, QuestionId = 10, Exam = null!, Question = null! }
            }.AsQueryable().BuildMock());

        // Act
        var (result, rejected) = await _service.UnassignQuestions(dto);

        // Assert
        result.Should().Be(ExamOperationResult.Success);
        rejected.Should().HaveCount(1);
        rejected.First().Id.Should().Be(30);
        rejected.First().Reason.Should().Be(RejectionReason.NotAssigned);
    }

    [Fact]
    [Trait("Category", TestCategories.Business)]
    public async Task UnassignQuestions_ExamNotFound_ReturnsNotFound()
    {
        // Arrange
        var dto = new AssignQuestionsDto { ExamId = 999, QuestionIds = new List<int> { 10 } };

        _examRepoMock
            .Setup(x => x.GetByCondition(It.IsAny<Expression<Func<Exam, bool>>>()))
            .Returns(new List<Exam>().AsQueryable().BuildMock());

        // Act
        var (result, rejected) = await _service.UnassignQuestions(dto);

        // Assert
        result.Should().Be(ExamOperationResult.NotFound);
        rejected.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", TestCategories.Business)]
    public async Task UnassignQuestions_ArchivedExam_ReturnsExamArchived()
    {
        // Arrange
        var dto = new AssignQuestionsDto { ExamId = 1, QuestionIds = new List<int> { 10 } };

        _examRepoMock
            .Setup(x => x.GetByCondition(It.IsAny<Expression<Func<Exam, bool>>>()))
            .Returns(new List<Exam>
            {
                new Exam { ID = 1, ExamStatus = ExamStatus.Archived, Course = null! }
            }.AsQueryable().BuildMock());

        // Act
        var (result, rejected) = await _service.UnassignQuestions(dto);

        // Assert
        result.Should().Be(ExamOperationResult.ExamArchived);
        rejected.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", TestCategories.Business)]
    public async Task UnassignQuestions_HasSubmissions_ReturnsHasSubmissions()
    {
        // Arrange
        var dto = new AssignQuestionsDto { ExamId = 1, QuestionIds = new List<int> { 10 } };

        _examRepoMock
            .Setup(x => x.GetByCondition(It.IsAny<Expression<Func<Exam, bool>>>()))
            .Returns(new List<Exam>
            {
                new Exam { ID = 1, ExamStatus = ExamStatus.Published, ExamAttempts = new List<ExamAttempt> { new ExamAttempt { Exam = null!, Student = null! } }, Course = null! }
            }.AsQueryable().BuildMock());

        // Act
        var (result, rejected) = await _service.UnassignQuestions(dto);

        // Assert
        result.Should().Be(ExamOperationResult.HasSubmissions);
        rejected.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", TestCategories.Business)]
    public async Task UnassignQuestions_NoneAssigned_ReturnsSuccessWithAllRejected()
    {
        // Arrange
        var dto = new AssignQuestionsDto { ExamId = 1, QuestionIds = new List<int> { 10, 20 } };

        _examRepoMock
            .Setup(x => x.GetByCondition(It.IsAny<Expression<Func<Exam, bool>>>()))
            .Returns(new List<Exam>
            {
                new Exam { ID = 1, ExamStatus = ExamStatus.Draft, ExamAttempts = new List<ExamAttempt>(), Course = null! }
            }.AsQueryable().BuildMock());

        _examQuestionRepoMock
            .Setup(x => x.GetByCondition(It.IsAny<Expression<Func<ExamQuestion, bool>>>()))
            .Returns(new List<ExamQuestion>().AsQueryable().BuildMock());

        // Act
        var (result, rejected) = await _service.UnassignQuestions(dto);

        // Assert
        result.Should().Be(ExamOperationResult.Success);
        rejected.Should().HaveCount(2);
        rejected.Should().AllSatisfy(r => r.Reason.Should().Be(RejectionReason.NotAssigned));
        _examQuestionRepoMock.Verify(x => x.DeleteRange(It.IsAny<IEnumerable<ExamQuestion>>()), Times.Never);
    }

    #endregion

    #region Delete Tests

    [Fact]
    [Trait("Category", TestCategories.Happy)]
    public async Task Delete_DraftOrArchivedExams_DeletesAndReturnsFailedIds()
    {
        // Arrange
        var idsToDelete = new List<int> { 1, 2, 3 };

        var exams = new List<Exam>
        {
            new Exam { ID = 1, ExamStatus = ExamStatus.Draft },
            new Exam { ID = 2, ExamStatus = ExamStatus.Archived }
        };

        _examRepoMock
            .Setup(x => x.GetByCondition(It.IsAny<Expression<Func<Exam, bool>>>()))
            .Returns(exams.AsQueryable().BuildMock());

        // Act
        var failedIds = await _service.Delete(idsToDelete);

        // Assert
        failedIds.Should().ContainSingle().Which.Should().Be(3);
        _examRepoMock.Verify(x => x.DeleteRange(It.Is<List<Exam>>(e => e.Count == 2)), Times.Once);
        _examRepoMock.Verify(x => x.SaveChanges(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    [Trait("Category", TestCategories.Business)]
    public async Task Delete_PublishedExams_ReturnsFailedIds()
    {
        // Arrange
        var idsToDelete = new List<int> { 1 };

        _examRepoMock
            .Setup(x => x.GetByCondition(It.IsAny<Expression<Func<Exam, bool>>>()))
            .Returns(new List<Exam>().AsQueryable().BuildMock()); // Published exams are filtered out

        // Act
        var failedIds = await _service.Delete(idsToDelete);

        // Assert
        failedIds.Should().BeEquivalentTo(new[] { 1 });
        _examRepoMock.Verify(x => x.DeleteRange(It.IsAny<IEnumerable<Exam>>()), Times.Once); // Called with empty list
    }

    #endregion
}
