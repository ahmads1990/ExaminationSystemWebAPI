using ExaminationSystem.Application.DTOs.Choices;
using ExaminationSystem.Application.DTOs.Questions;
using ExaminationSystem.Application.Services;
using ExaminationSystem.Domain.Entities;
using ExaminationSystem.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MockQueryable;
using Moq;
using System.Linq.Expressions;

namespace ExaminationSystem.UnitTests.Services;

public class QuestionServiceTests
{
    private readonly Mock<IRepository<Question>> _questionRepoMock;
    private readonly Mock<IRepository<Choice>> _choiceRepoMock;
    private readonly Mock<ILogger<QuestionService>> _loggerMock;
    private readonly QuestionService _service;

    public QuestionServiceTests()
    {
        _questionRepoMock = new Mock<IRepository<Question>>();
        _choiceRepoMock = new Mock<IRepository<Choice>>();
        _loggerMock = new Mock<ILogger<QuestionService>>();
        _service = new QuestionService(_questionRepoMock.Object, _choiceRepoMock.Object, _loggerMock.Object);
    }

    #region GetAll Tests

    [Fact]
    [Trait("Category", TestCategories.Happy)]
    public async Task GetAll_ReturnsPaginatedAndFilteredResults()
    {
        // Arrange
        var listDto = new ListQuestionsDto
        {
            PageIndex = 0,
            PageSize = 10,
            Body = "Math",
            OrderBy = nameof(Question.Score),
            SortDirection = SortingDirection.Descending
        };

        var questions = new List<Question>
        {
            new Question { ID = 1, Body = "Math Q1", Score = 5 },
            new Question { ID = 2, Body = "Math Q2", Score = 10 },
            new Question { ID = 3, Body = "Physics Q1", Score = 5 }
        };

        _questionRepoMock
            .Setup(x => x.GetAll())
            .Returns(questions.AsQueryable().BuildMock());

        // Act
        var (data, totalCount) = await _service.GetAll(listDto);

        // Assert
        totalCount.Should().Be(2); // Only Math questions
        data.Should().HaveCount(2);
        data.First().ID.Should().Be(2); // Score 10 should be descending first
    }

    #endregion

    #region GetByID Tests

    [Fact]
    [Trait("Category", TestCategories.Happy)]
    public async Task GetByID_ExistingQuestion_ReturnsMappedDto()
    {
        // Arrange
        var question = new Question
        {
            ID = 1,
            Body = "Sample Question",
            Score = 5,
            QuestionLevel = QuestionLevel.Medium,
            Choices = new List<Choice>
            {
                new Choice { ID = 10, Body = "Choice A", IsCorrect = true },
                new Choice { ID = 11, Body = "Choice B", IsCorrect = false }
            }
        };

        _questionRepoMock
            .Setup(x => x.GetByID(1))
            .Returns(new List<Question> { question }.AsQueryable().BuildMock());

        // Act
        var result = await _service.GetByID(1);

        // Assert
        result.Should().NotBeNull();
        result!.ID.Should().Be(1);
        result.Body.Should().Be("Sample Question");
        result.Choices.Should().HaveCount(2);
    }

    [Fact]
    [Trait("Category", TestCategories.Business)]
    public async Task GetByID_NonExistingQuestion_ReturnsNull()
    {
        // Arrange
        _questionRepoMock
            .Setup(x => x.GetByID(999))
            .Returns(new List<Question>().AsQueryable().BuildMock());

        // Act
        var result = await _service.GetByID(999);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region Add Tests

    [Fact]
    [Trait("Category", TestCategories.Happy)]
    public async Task Add_ValidDto_ReturnsSuccess()
    {
        // Arrange
        var dto = new AddQuestionDto
        {
            Body = "What is 2+2?",
            Score = 5,
            QuestionLevel = QuestionLevel.Easy,
            Choices = new List<AddChoiceDto>
            {
                new AddChoiceDto { Body = "3", IsCorrect = false },
                new AddChoiceDto { Body = "4", IsCorrect = true }
            }
        };

        _questionRepoMock
            .Setup(x => x.Add(It.IsAny<Question>(), It.IsAny<CancellationToken>()))
            .Callback<Question, CancellationToken>((q, _) => q.ID = 10);

        // Act
        var result = await _service.Add(dto);

        // Assert
        result.Should().Be(QuestionOperationResult.Success);
        _questionRepoMock.Verify(x => x.Add(It.IsAny<Question>(), It.IsAny<CancellationToken>()), Times.Once);
        _questionRepoMock.Verify(x => x.SaveChanges(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Update Tests

    [Fact]
    [Trait("Category", TestCategories.Happy)]
    public async Task Update_ExistsWithChoices_ReturnsSuccess()
    {
        // Arrange
        var dto = new UpdateQuestionDto
        {
            ID = 1,
            Body = "Updated body",
            Score = 8,
            QuestionLevel = QuestionLevel.Hard,
            Choices = new List<UpdateChoiceDto>
            {
                new UpdateChoiceDto { ID = 100, Body = "Updated choice", IsCorrect = true },
                new UpdateChoiceDto { ID = 0, Body = "New choice", IsCorrect = false }
            }
        };

        var existingQuestion = new Question
        {
            ID = 1,
            Body = "Original",
            Score = 5,
            QuestionLevel = QuestionLevel.Easy,
            Choices = new List<Choice>
            {
                new Choice { ID = 100, Body = "Old choice", IsCorrect = false, QuestionId = 1 },
                new Choice { ID = 200, Body = "To be removed", IsCorrect = false, QuestionId = 1 }
            }
        };

        _questionRepoMock
            .Setup(x => x.GetByID(1))
            .Returns(new List<Question> { existingQuestion }.AsQueryable().BuildMock());

        // Act
        var result = await _service.Update(dto);

        // Assert
        result.Should().Be(QuestionOperationResult.Success);
        _choiceRepoMock.Verify(x => x.Delete(It.Is<Choice>(c => c.ID == 200)), Times.Once);
        _questionRepoMock.Verify(x => x.Update(It.IsAny<Question>()), Times.Once);
        _questionRepoMock.Verify(x => x.SaveChanges(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    [Trait("Category", TestCategories.Business)]
    public async Task Update_NotFound_ReturnsNotFound()
    {
        // Arrange
        var dto = new UpdateQuestionDto { ID = 999, Body = "Body" };

        _questionRepoMock
            .Setup(x => x.GetByID(999))
            .Returns(new List<Question>().AsQueryable().BuildMock());

        // Act
        var result = await _service.Update(dto);

        // Assert
        result.Should().Be(QuestionOperationResult.NotFound);
        _questionRepoMock.Verify(x => x.Update(It.IsAny<Question>()), Times.Never);
    }

    [Fact]
    [Trait("Category", TestCategories.Happy)]
    public async Task Update_AddsNewChoicesAndUpdatesExisting()
    {
        // Arrange
        var dto = new UpdateQuestionDto
        {
            ID = 1,
            Body = "Q",
            Score = 5,
            QuestionLevel = QuestionLevel.Medium,
            Choices = new List<UpdateChoiceDto>
            {
                new UpdateChoiceDto { ID = 10, Body = "Updated", IsCorrect = true },
                new UpdateChoiceDto { ID = 0, Body = "Brand new", IsCorrect = false }
            }
        };

        var existing = new Question
        {
            ID = 1,
            Body = "Q",
            Score = 5,
            QuestionLevel = QuestionLevel.Easy,
            Choices = new List<Choice> { new Choice { ID = 10, Body = "Old", IsCorrect = false, QuestionId = 1 } }
        };

        _questionRepoMock
            .Setup(x => x.GetByID(1))
            .Returns(new List<Question> { existing }.AsQueryable().BuildMock());

        // Act
        var result = await _service.Update(dto);

        // Assert
        result.Should().Be(QuestionOperationResult.Success);
        existing.Choices.Should().HaveCount(2);
        existing.Choices.First(c => c.ID == 10).Body.Should().Be("Updated");
        existing.Choices.First(c => c.ID == 10).IsCorrect.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", TestCategories.Business)]
    public async Task Update_QuestionLocked_ReturnsLocked()
    {
        // Arrange
        var dto = new UpdateQuestionDto { ID = 1, Body = "Q" };

        var existingQuestion = new Question { ID = 1 };
        existingQuestion.ExamQuestions = new List<ExamQuestion>
        {
            new ExamQuestion
            {
                Exam = new Exam
                {
                    ExamAttempts = new List<ExamAttempt> { new ExamAttempt { ID = 1 } }
                },
                Question = existingQuestion
            }
        };

        _questionRepoMock
            .Setup(x => x.GetByID(1))
            .Returns(new List<Question> { existingQuestion }.AsQueryable().BuildMock());

        // Act
        var result = await _service.Update(dto);

        // Assert
        result.Should().Be(QuestionOperationResult.Locked);
        _questionRepoMock.Verify(x => x.Update(It.IsAny<Question>()), Times.Never);
    }

    #endregion

    #region Delete Tests

    [Fact]
    [Trait("Category", TestCategories.Happy)]
    public async Task Delete_UnassignedQuestions_DeletesAndReturnsFailedIds()
    {
        // Arrange
        var idsToDelete = new List<int> { 1, 2, 3 };

        var deletableQuestions = new List<Question>
        {
            new Question { ID = 1, Body = "Q1", Choices = new List<Choice> { new Choice { ID = 10, QuestionId = 1 } } },
            new Question { ID = 2, Body = "Q2", Choices = new List<Choice>() }
        };

        _questionRepoMock
            .Setup(x => x.GetByCondition(It.IsAny<Expression<Func<Question, bool>>>()))
            .Returns(deletableQuestions.AsQueryable().BuildMock());

        // Act
        var failedIds = await _service.Delete(idsToDelete);

        // Assert
        failedIds.Should().ContainSingle().Which.Should().Be(3);
        _choiceRepoMock.Verify(x => x.DeleteRange(It.IsAny<ICollection<Choice>>()), Times.Exactly(2));
        _questionRepoMock.Verify(x => x.DeleteRange(It.Is<List<Question>>(q => q.Count == 2)), Times.Once);
    }

    [Fact]
    [Trait("Category", TestCategories.Business)]
    public async Task Delete_AllAssignedToExams_ReturnsAllAsFailedIds()
    {
        // Arrange
        var idsToDelete = new List<int> { 1, 2 };

        _questionRepoMock
            .Setup(x => x.GetByCondition(It.IsAny<Expression<Func<Question, bool>>>()))
            .Returns(new List<Question>().AsQueryable().BuildMock());

        // Act
        var failedIds = await _service.Delete(idsToDelete);

        // Assert
        failedIds.Should().BeEquivalentTo(new[] { 1, 2 });
    }

    #endregion
}
