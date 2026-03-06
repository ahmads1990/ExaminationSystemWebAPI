using ExaminationSystem.Application.DTOs.Auth;
using ExaminationSystem.Application.DTOs.StudentExams;
using ExaminationSystem.Application.Interfaces;
using ExaminationSystem.Application.Services;
using ExaminationSystem.Domain.Entities;
using ExaminationSystem.Domain.Interfaces;
using FluentAssertions;
using Hangfire;
using MockQueryable;
using Moq;
using System.Linq.Expressions;

namespace ExaminationSystem.UnitTests.Services;

public class StudentExamServiceTests
{
    private readonly Mock<IRepository<Exam>> _examRepoMock;
    private readonly Mock<IRepository<Student>> _studentRepoMock;
    private readonly Mock<IRepository<ExamAttempt>> _attemptRepoMock;
    private readonly Mock<IRepository<StudentCourses>> _studentCoursesRepoMock;
    private readonly Mock<IRepository<StudentExamsAnswers>> _answersRepoMock;
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly Mock<IBackgroundJobClient> _backgroundJobClientMock;
    private readonly StudentExamService _service;

    public StudentExamServiceTests()
    {
        _examRepoMock = new Mock<IRepository<Exam>>();
        _studentRepoMock = new Mock<IRepository<Student>>();
        _attemptRepoMock = new Mock<IRepository<ExamAttempt>>();
        _studentCoursesRepoMock = new Mock<IRepository<StudentCourses>>();
        _answersRepoMock = new Mock<IRepository<StudentExamsAnswers>>();
        _authServiceMock = new Mock<IAuthService>();
        _backgroundJobClientMock = new Mock<IBackgroundJobClient>();
        _service = new StudentExamService(
            _examRepoMock.Object, _studentRepoMock.Object,
            _attemptRepoMock.Object, _studentCoursesRepoMock.Object,
            _answersRepoMock.Object, _authServiceMock.Object,
            _backgroundJobClientMock.Object);
    }

    #region StartExamAttempt Tests

    [Fact]
    [Trait("Category", TestCategories.Happy)]
    public async Task StartExamAttempt_ValidRequest_ReturnsSuccessWithToken()
    {
        var dto = new StartExamAttemptDto { StudentId = 1, ExamId = 10 };
        var exam = CreatePublishedExam(10, courseId: 5);

        _studentRepoMock.Setup(x => x.CheckExistsByID(1, default)).ReturnsAsync(true);
        _examRepoMock.Setup(x => x.GetByID(10))
            .Returns(new List<Exam> { exam }.AsQueryable().BuildMock());
        _studentCoursesRepoMock.Setup(x => x.CheckExistsByCondition(It.IsAny<Expression<Func<StudentCourses, bool>>>(), default))
            .ReturnsAsync(true);
        _attemptRepoMock.Setup(x => x.SaveChanges(default)).ReturnsAsync(true);
        _authServiceMock.Setup(x => x.CreateExamAttemptToken(It.IsAny<CreateExamTokenDto>(), default))
            .ReturnsAsync((UserOperationResult.Success, "exam.jwt.token"));

        var (result, token) = await _service.StartExamAttempt(dto);

        result.Should().Be(StudentExamAttemptResult.Success);
        token.Should().Be("exam.jwt.token");
        _attemptRepoMock.Verify(x => x.Add(It.IsAny<ExamAttempt>(), default), Times.Once);
    }

    [Fact]
    [Trait("Category", TestCategories.Business)]
    public async Task StartExamAttempt_StudentNotFound_ReturnsStudentNotFound()
    {
        var dto = new StartExamAttemptDto { StudentId = 999, ExamId = 10 };
        _studentRepoMock.Setup(x => x.CheckExistsByID(999, default)).ReturnsAsync(false);

        var (result, token) = await _service.StartExamAttempt(dto);

        result.Should().Be(StudentExamAttemptResult.StudentNotFound);
        token.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", TestCategories.Business)]
    public async Task StartExamAttempt_ExamNotPublished_ReturnsExamNotPublished()
    {
        var dto = new StartExamAttemptDto { StudentId = 1, ExamId = 10 };
        var exam = CreatePublishedExam(10, courseId: 5);
        exam.ExamStatus = ExamStatus.Draft;

        _studentRepoMock.Setup(x => x.CheckExistsByID(1, default)).ReturnsAsync(true);
        _examRepoMock.Setup(x => x.GetByID(10))
            .Returns(new List<Exam> { exam }.AsQueryable().BuildMock());

        var (result, _) = await _service.StartExamAttempt(dto);

        result.Should().Be(StudentExamAttemptResult.ExamNotPublished);
    }

    [Fact]
    [Trait("Category", TestCategories.Business)]
    public async Task StartExamAttempt_DeadlinePassed_ReturnsExamDeadlinePassed()
    {
        var dto = new StartExamAttemptDto { StudentId = 1, ExamId = 10 };
        var exam = CreatePublishedExam(10, courseId: 5);
        exam.DeadlineDate = DateTime.UtcNow.AddDays(-1);

        _studentRepoMock.Setup(x => x.CheckExistsByID(1, default)).ReturnsAsync(true);
        _examRepoMock.Setup(x => x.GetByID(10))
            .Returns(new List<Exam> { exam }.AsQueryable().BuildMock());

        var (result, _) = await _service.StartExamAttempt(dto);

        result.Should().Be(StudentExamAttemptResult.ExamDeadlinePassed);
    }

    [Fact]
    [Trait("Category", TestCategories.Business)]
    public async Task StartExamAttempt_NotEnrolled_ReturnsNotEnrolled()
    {
        var dto = new StartExamAttemptDto { StudentId = 1, ExamId = 10 };
        var exam = CreatePublishedExam(10, courseId: 5);

        _studentRepoMock.Setup(x => x.CheckExistsByID(1, default)).ReturnsAsync(true);
        _examRepoMock.Setup(x => x.GetByID(10))
            .Returns(new List<Exam> { exam }.AsQueryable().BuildMock());
        _studentCoursesRepoMock.Setup(x => x.CheckExistsByCondition(It.IsAny<Expression<Func<StudentCourses, bool>>>(), default))
            .ReturnsAsync(false);

        var (result, _) = await _service.StartExamAttempt(dto);

        result.Should().Be(StudentExamAttemptResult.NotEnrolled);
    }

    [Fact]
    [Trait("Category", TestCategories.Business)]
    public async Task StartExamAttempt_HasActiveAttempt_ReturnsHasActiveAttempt()
    {
        var dto = new StartExamAttemptDto { StudentId = 1, ExamId = 10 };
        var exam = CreatePublishedExam(10, courseId: 5);
        exam.ExamAttempts.Add(new ExamAttempt { ID = 100, StudentId = 1, ExamAttemptStatus = ExamAttemptStatus.InProgress });

        _studentRepoMock.Setup(x => x.CheckExistsByID(1, default)).ReturnsAsync(true);
        _examRepoMock.Setup(x => x.GetByID(10))
            .Returns(new List<Exam> { exam }.AsQueryable().BuildMock());
        _studentCoursesRepoMock.Setup(x => x.CheckExistsByCondition(It.IsAny<Expression<Func<StudentCourses, bool>>>(), default))
            .ReturnsAsync(true);

        var (result, _) = await _service.StartExamAttempt(dto);

        result.Should().Be(StudentExamAttemptResult.HasActiveAttempt);
    }

    [Fact]
    [Trait("Category", TestCategories.Business)]
    public async Task StartExamAttempt_MaxAttemptsReached_ReturnsMaxAttemptsExceeded()
    {
        var dto = new StartExamAttemptDto { StudentId = 1, ExamId = 10 };
        var exam = CreatePublishedExam(10, courseId: 5, maxAttempts: 1);
        exam.ExamAttempts.Add(new ExamAttempt { ID = 100, StudentId = 1, ExamAttemptStatus = ExamAttemptStatus.Completed });

        _studentRepoMock.Setup(x => x.CheckExistsByID(1, default)).ReturnsAsync(true);
        _examRepoMock.Setup(x => x.GetByID(10))
            .Returns(new List<Exam> { exam }.AsQueryable().BuildMock());
        _studentCoursesRepoMock.Setup(x => x.CheckExistsByCondition(It.IsAny<Expression<Func<StudentCourses, bool>>>(), default))
            .ReturnsAsync(true);

        var (result, _) = await _service.StartExamAttempt(dto);

        result.Should().Be(StudentExamAttemptResult.MaxAttemptsExceeded);
    }

    #endregion

    #region GetExamQuestions Tests

    [Fact]
    [Trait("Category", TestCategories.Happy)]
    public async Task GetExamQuestions_ValidAttempt_ReturnsQuestionsWithoutIsCorrect()
    {
        var attempt = CreateInProgressAttempt(1, studentId: 1);
        _attemptRepoMock.Setup(x => x.GetByID(1))
            .Returns(new List<ExamAttempt> { attempt }.AsQueryable().BuildMock());

        var (result, questions) = await _service.GetExamQuestions(1, 1);

        result.Should().Be(StudentExamAttemptResult.Success);
        questions.Should().HaveCount(2);
        questions![0].Choices[0].Should().BeOfType<ExamChoiceDto>();
    }

    [Fact]
    [Trait("Category", TestCategories.Business)]
    public async Task GetExamQuestions_WrongStudent_ReturnsExamNotFound()
    {
        var attempt = CreateInProgressAttempt(1, studentId: 1);
        _attemptRepoMock.Setup(x => x.GetByID(1))
            .Returns(new List<ExamAttempt> { attempt }.AsQueryable().BuildMock());

        var (result, _) = await _service.GetExamQuestions(1, 999);

        result.Should().Be(StudentExamAttemptResult.ExamNotFound);
    }

    [Fact]
    [Trait("Category", TestCategories.Business)]
    public async Task GetExamQuestions_CompletedAttempt_ReturnsAttemptAlreadyCompleted()
    {
        var attempt = CreateInProgressAttempt(1, studentId: 1);
        attempt.ExamAttemptStatus = ExamAttemptStatus.Completed;
        _attemptRepoMock.Setup(x => x.GetByID(1))
            .Returns(new List<ExamAttempt> { attempt }.AsQueryable().BuildMock());

        var (result, _) = await _service.GetExamQuestions(1, 1);

        result.Should().Be(StudentExamAttemptResult.AttemptAlreadyCompleted);
    }

    #endregion

    #region SubmitAnswer Tests

    [Fact]
    [Trait("Category", TestCategories.Happy)]
    public async Task SubmitAnswer_ValidAnswer_SavesAndReturnsSuccess()
    {
        var answer = new SubmitAnswerDto { QuestionId = 100, ChoiceId = 1001 };
        var attempt = new ExamAttempt { ID = 1, StudentId = 1, ExamAttemptStatus = ExamAttemptStatus.InProgress };

        _attemptRepoMock.Setup(x => x.GetByID(1, default)).ReturnsAsync(attempt);
        _answersRepoMock.Setup(x => x.SaveChanges(default)).ReturnsAsync(true);

        var result = await _service.SubmitAnswer(answer, 1, 1);

        result.Should().Be(StudentExamAttemptResult.Success);
        _answersRepoMock.Verify(x => x.Add(It.IsAny<StudentExamsAnswers>(), default), Times.Once);
    }

    [Fact]
    [Trait("Category", TestCategories.Business)]
    public async Task SubmitAnswer_CompletedAttempt_ReturnsAttemptAlreadyCompleted()
    {
        var answer = new SubmitAnswerDto { QuestionId = 100, ChoiceId = 1001 };
        var attempt = new ExamAttempt { ID = 1, StudentId = 1, ExamAttemptStatus = ExamAttemptStatus.Completed };

        _attemptRepoMock.Setup(x => x.GetByID(1, default)).ReturnsAsync(attempt);

        var result = await _service.SubmitAnswer(answer, 1, 1);

        result.Should().Be(StudentExamAttemptResult.AttemptAlreadyCompleted);
        _answersRepoMock.Verify(x => x.Add(It.IsAny<StudentExamsAnswers>(), default), Times.Never);
    }

    #endregion

    #region SubmitAnswers (Batch) Tests

    [Fact]
    [Trait("Category", TestCategories.Happy)]
    public async Task SubmitAnswers_ValidBatch_SavesAllAndReturnsSuccess()
    {
        var answers = new List<SubmitAnswerDto>
        {
            new() { QuestionId = 100, ChoiceId = 1001 },
            new() { QuestionId = 200, ChoiceId = 2001 }
        };
        var attempt = new ExamAttempt { ID = 1, StudentId = 1, ExamAttemptStatus = ExamAttemptStatus.InProgress };

        _attemptRepoMock.Setup(x => x.GetByID(1, default)).ReturnsAsync(attempt);
        _answersRepoMock.Setup(x => x.SaveChanges(default)).ReturnsAsync(true);

        var result = await _service.SubmitAnswers(answers, 1, 1);

        result.Should().Be(StudentExamAttemptResult.Success);
        _answersRepoMock.Verify(x => x.AddRange(It.IsAny<IEnumerable<StudentExamsAnswers>>(), default), Times.Once);
    }

    [Fact]
    [Trait("Category", TestCategories.Business)]
    public async Task SubmitAnswers_WrongStudent_ReturnsExamNotFound()
    {
        var attempt = new ExamAttempt { ID = 1, StudentId = 1, ExamAttemptStatus = ExamAttemptStatus.InProgress };
        _attemptRepoMock.Setup(x => x.GetByID(1, default)).ReturnsAsync(attempt);

        var result = await _service.SubmitAnswers(new List<SubmitAnswerDto>(), 1, 999);

        result.Should().Be(StudentExamAttemptResult.ExamNotFound);
    }

    #endregion

    #region SubmitAttempt Tests

    [Fact]
    [Trait("Category", TestCategories.Happy)]
    public async Task SubmitAttempt_ValidInProgressAttempt_SetsCompletedAndReturnsSuccess()
    {
        var attempt = new ExamAttempt { ID = 1, StudentId = 1, ExamAttemptStatus = ExamAttemptStatus.InProgress };

        _attemptRepoMock.Setup(x => x.GetByID(1))
            .Returns(new List<ExamAttempt> { attempt }.AsQueryable().BuildMock());
        _attemptRepoMock.Setup(x => x.SaveChanges(default)).ReturnsAsync(true);

        var result = await _service.SubmitAttempt(1, 1);

        result.Should().Be(StudentExamAttemptResult.Success);
        attempt.ExamAttemptStatus.Should().Be(ExamAttemptStatus.Completed);
        attempt.EndTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        _attemptRepoMock.Verify(x => x.Update(attempt), Times.Once);
    }

    [Fact]
    [Trait("Category", TestCategories.Business)]
    public async Task SubmitAttempt_AlreadyCompleted_ReturnsAttemptAlreadyCompleted()
    {
        var attempt = new ExamAttempt { ID = 1, StudentId = 1, ExamAttemptStatus = ExamAttemptStatus.Completed };
        _attemptRepoMock.Setup(x => x.GetByID(1))
            .Returns(new List<ExamAttempt> { attempt }.AsQueryable().BuildMock());

        var result = await _service.SubmitAttempt(1, 1);

        result.Should().Be(StudentExamAttemptResult.AttemptAlreadyCompleted);
        _attemptRepoMock.Verify(x => x.Update(It.IsAny<ExamAttempt>()), Times.Never);
    }

    [Fact]
    [Trait("Category", TestCategories.Business)]
    public async Task SubmitAttempt_WrongStudent_ReturnsExamNotFound()
    {
        var attempt = new ExamAttempt { ID = 1, StudentId = 1, ExamAttemptStatus = ExamAttemptStatus.InProgress };
        _attemptRepoMock.Setup(x => x.GetByID(1))
            .Returns(new List<ExamAttempt> { attempt }.AsQueryable().BuildMock());

        var result = await _service.SubmitAttempt(1, 999);

        result.Should().Be(StudentExamAttemptResult.ExamNotFound);
    }

    #endregion

    #region Helpers

    private static Exam CreatePublishedExam(int id, int courseId, int maxAttempts = 3)
    {
        return new Exam
        {
            ID = id,
            CourseID = courseId,
            ExamStatus = ExamStatus.Published,
            DeadlineDate = DateTime.UtcNow.AddDays(7),
            MaxAttempts = maxAttempts,
            MaxDurationInMinutes = 60,
            TotalGrade = 100,
            PassingScore = 50,
            ExamAttempts = new List<ExamAttempt>()
        };
    }

    private static ExamAttempt CreateInProgressAttempt(int attemptId, int studentId, bool shuffleQuestions = false)
    {
        var q1 = new Question
        {
            ID = 100,
            Body = "Q1",
            Score = 5,
            Choices = new List<Choice>
            {
                new() { ID = 1001, Body = "Correct", IsCorrect = true, QuestionId = 100 },
                new() { ID = 1002, Body = "Wrong", IsCorrect = false, QuestionId = 100 }
            }
        };
        var q2 = new Question
        {
            ID = 200,
            Body = "Q2",
            Score = 10,
            Choices = new List<Choice>
            {
                new() { ID = 2001, Body = "Correct", IsCorrect = true, QuestionId = 200 },
                new() { ID = 2002, Body = "Wrong", IsCorrect = false, QuestionId = 200 }
            }
        };

        var exam = new Exam
        {
            ID = 10,
            TotalGrade = 15,
            PassingScore = 10,
            ShuffleQuestions = shuffleQuestions,
            ExamQuestions = new List<ExamQuestion>
            {
                new() { ExamId = 10, QuestionId = 100, Question = q1, Exam = null! },
                new() { ExamId = 10, QuestionId = 200, Question = q2, Exam = null! }
            }
        };

        foreach (var eq in exam.ExamQuestions)
            eq.Exam = exam;

        return new ExamAttempt
        {
            ID = attemptId,
            StudentId = studentId,
            ExamId = 10,
            Exam = exam,
            ExamAttemptStatus = ExamAttemptStatus.InProgress,
            Answers = new List<StudentExamsAnswers>()
        };
    }

    #endregion
}
