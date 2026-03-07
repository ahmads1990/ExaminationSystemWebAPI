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
}
