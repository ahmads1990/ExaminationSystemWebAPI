using ExaminationSystem.Application.DTOs.Auth;
using ExaminationSystem.Application.DTOs.StudentExams;
using ExaminationSystem.Application.Interfaces;
using ExaminationSystem.Domain.Entities;
using ExaminationSystem.Domain.Interfaces;
using Hangfire;
using Microsoft.EntityFrameworkCore;

namespace ExaminationSystem.Application.Services;

/// <summary>
/// Manages student exam attempt lifecycle: starting, answering, submitting, and auto-closing.
/// </summary>
public class StudentExamService : IStudentExamService
{
    #region Fields

    private readonly IRepository<Exam> _examRepo;
    private readonly IRepository<Student> _studentRepo;
    private readonly IRepository<ExamAttempt> _examAttemptRepo;
    private readonly IRepository<StudentCourses> _studentCoursesRepo;
    private readonly IRepository<StudentExamsAnswers> _answersRepo;
    private readonly IAuthService _authService;
    private readonly IBackgroundJobClient _backgroundJobClient;

    #endregion

    #region Constructors

    public StudentExamService(
        IRepository<Exam> examRepo,
        IRepository<Student> studentRepo,
        IRepository<ExamAttempt> examAttemptRepo,
        IRepository<StudentCourses> studentCoursesRepo,
        IRepository<StudentExamsAnswers> answersRepo,
        IAuthService authService,
        IBackgroundJobClient backgroundJobClient)
    {
        _examRepo = examRepo;
        _studentRepo = studentRepo;
        _examAttemptRepo = examAttemptRepo;
        _studentCoursesRepo = studentCoursesRepo;
        _answersRepo = answersRepo;
        _authService = authService;
        _backgroundJobClient = backgroundJobClient;
    }

    #endregion

    #region Public Methods

    /// <inheritdoc/>
    public async Task<(StudentExamAttemptResult Result, string AccessToken)> StartExamAttempt(StartExamAttemptDto startExamDto, CancellationToken cancellationToken = default)
    {
        var (createResult, examTokenInfoDto) = await CreateExamAttempt(startExamDto, cancellationToken);
        if (createResult != StudentExamAttemptResult.Success || examTokenInfoDto is null)
            return (createResult, string.Empty);

        EnqueueAutoCloseJob(examTokenInfoDto);

        var (tokenResult, accessToken) = await _authService.CreateExamAttemptToken(examTokenInfoDto, cancellationToken);
        if (tokenResult != UserOperationResult.Success)
            return (StudentExamAttemptResult.UnknownError, string.Empty);

        return (StudentExamAttemptResult.Success, accessToken);
    }

    /// <inheritdoc/>
    public async Task<(StudentExamAttemptResult Result, List<ExamQuestionDto>? Questions)> GetExamQuestions(int examAttemptId, int studentId, CancellationToken cancellationToken = default)
    {
        var attempt = await _examAttemptRepo.GetByID(examAttemptId)
            .Include(a => a.Exam)
                .ThenInclude(e => e.ExamQuestions)
                .ThenInclude(eq => eq.Question)
                .ThenInclude(q => q.Choices)
            .FirstOrDefaultAsync(cancellationToken);

        if (attempt is null || attempt.StudentId != studentId)
            return (StudentExamAttemptResult.ExamNotFound, null);

        if (attempt.ExamAttemptStatus != ExamAttemptStatus.InProgress)
            return (StudentExamAttemptResult.AttemptAlreadyCompleted, null);

        var questions = attempt.Exam.ExamQuestions.Select(eq => new ExamQuestionDto
        {
            QuestionId = eq.Question.ID,
            Body = eq.Question.Body,
            Score = eq.Question.Score,
            Choices = eq.Question.Choices.Select(c => new ExamChoiceDto
            {
                ChoiceId = c.ID,
                Body = c.Body
            }).ToList()
        }).ToList();

        if (attempt.Exam.ShuffleQuestions)
        {
            var rng = new Random();
            questions = questions.OrderBy(_ => rng.Next()).ToList();
        }

        return (StudentExamAttemptResult.Success, questions);
    }

    /// <inheritdoc/>
    public async Task<StudentExamAttemptResult> SubmitAnswer(SubmitAnswerDto answerDto, int examAttemptId, int studentId, CancellationToken cancellationToken = default)
    {
        var validationResult = await ValidateAttemptForSubmission(examAttemptId, studentId, cancellationToken);
        if (validationResult != StudentExamAttemptResult.Success)
            return validationResult;

        var answer = new StudentExamsAnswers
        {
            StudentID = studentId,
            QuestionID = answerDto.QuestionId,
            ChoiceID = answerDto.ChoiceId,
            ExamAttemptID = examAttemptId
        };

        await _answersRepo.Add(answer, cancellationToken);
        await _answersRepo.SaveChanges(cancellationToken);

        return StudentExamAttemptResult.Success;
    }

    /// <inheritdoc/>
    public async Task<StudentExamAttemptResult> SubmitAnswers(List<SubmitAnswerDto> answers, int examAttemptId, int studentId, CancellationToken cancellationToken = default)
    {
        var validationResult = await ValidateAttemptForSubmission(examAttemptId, studentId, cancellationToken);
        if (validationResult != StudentExamAttemptResult.Success)
            return validationResult;

        var entities = answers.Select(a => new StudentExamsAnswers
        {
            StudentID = studentId,
            QuestionID = a.QuestionId,
            ChoiceID = a.ChoiceId,
            ExamAttemptID = examAttemptId
        });

        await _answersRepo.AddRange(entities, cancellationToken);
        await _answersRepo.SaveChanges(cancellationToken);

        return StudentExamAttemptResult.Success;
    }

    /// <inheritdoc/>
    public async Task<StudentExamAttemptResult> SubmitAttempt(int examAttemptId, int studentId, CancellationToken cancellationToken = default)
    {
        var attempt = await _examAttemptRepo.GetByID(examAttemptId, cancellationToken);

        if (attempt is null || attempt.StudentId != studentId)
            return StudentExamAttemptResult.ExamNotFound;

        if (attempt.ExamAttemptStatus != ExamAttemptStatus.InProgress)
            return StudentExamAttemptResult.AttemptAlreadyCompleted;

        attempt.ExamAttemptStatus = ExamAttemptStatus.Completed;
        attempt.EndTime = DateTime.UtcNow;

        _examAttemptRepo.Update(attempt);
        await _examAttemptRepo.SaveChanges(cancellationToken);

        return StudentExamAttemptResult.Success;
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Validates that an attempt exists, belongs to the student, and is still in progress.
    /// </summary>
    /// <param name="examAttemptId">The exam attempt identifier.</param>
    /// <param name="studentId">The student identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The operation result.</returns>
    private async Task<StudentExamAttemptResult> ValidateAttemptForSubmission(int examAttemptId, int studentId, CancellationToken cancellationToken)
    {
        var attempt = await _examAttemptRepo.GetByID(examAttemptId, cancellationToken);

        if (attempt is null || attempt.StudentId != studentId)
            return StudentExamAttemptResult.ExamNotFound;

        return attempt.ExamAttemptStatus != ExamAttemptStatus.InProgress
            ? StudentExamAttemptResult.AttemptAlreadyCompleted
            : StudentExamAttemptResult.Success;
    }

    /// <summary>
    /// Schedules a Hangfire background job to automatically close the exam attempt.
    /// </summary>
    /// <param name="examTokenInfoDto">The DTO containing attempt information.</param>
    private void EnqueueAutoCloseJob(CreateExamTokenDto examTokenInfoDto)
    {
        // Enqueue auto-close job after exam duration expires → sets status to TimedOut
        _backgroundJobClient.Schedule<ICloseExamAttemptJob>(
            job => job.ExecuteAsync(examTokenInfoDto.ExamAttemptId, CancellationToken.None),
            TimeSpan.FromMinutes(examTokenInfoDto.MaxDurationInMinutes));
    }

    /// <summary>
    /// Validates business rules and creates a new exam attempt.
    /// </summary>
    /// <param name="startExamDto">The attempt details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A tuple containing the operation result and token details if successful.</returns>
    private async Task<(StudentExamAttemptResult Result, CreateExamTokenDto? TokenDto)> CreateExamAttempt(StartExamAttemptDto startExamDto, CancellationToken cancellationToken = default)
    {
        // Validate student exists
        var studentExists = await _studentRepo.CheckExistsByID(startExamDto.StudentId, cancellationToken);
        if (!studentExists)
            return (StudentExamAttemptResult.StudentNotFound, null);

        // Validate exam exists and load only this student's attempts
        var exam = await _examRepo.GetByID(startExamDto.ExamId)
                                  .Include(e => e.ExamAttempts.Where(ea => ea.StudentId == startExamDto.StudentId))
                                  .FirstOrDefaultAsync(cancellationToken);
        if (exam is null)
            return (StudentExamAttemptResult.ExamNotFound, null);

        if (exam.ExamStatus != ExamStatus.Published)
            return (StudentExamAttemptResult.ExamNotPublished, null);

        if (exam.DeadlineDate < DateTime.UtcNow)
            return (StudentExamAttemptResult.ExamDeadlinePassed, null);

        // Validate enrollment
        var isEnrolled = await _studentCoursesRepo.CheckExistsByCondition(
            sc => sc.StudentID == startExamDto.StudentId && sc.CourseID == exam.CourseID, cancellationToken);
        if (!isEnrolled)
            return (StudentExamAttemptResult.NotEnrolled, null);

        // Validate no concurrent active attempt
        var hasActiveAttempt = exam.ExamAttempts.Any(a => a.ExamAttemptStatus == ExamAttemptStatus.InProgress);
        if (hasActiveAttempt)
            return (StudentExamAttemptResult.HasActiveAttempt, null);

        // Validate max attempts
        if (exam.ExamAttempts.Count >= exam.MaxAttempts)
            return (StudentExamAttemptResult.MaxAttemptsExceeded, null);

        // Create attempt
        var attempt = new ExamAttempt
        {
            ExamId = startExamDto.ExamId,
            StudentId = startExamDto.StudentId,
            StartTime = DateTime.UtcNow,
            ExamAttemptStatus = ExamAttemptStatus.InProgress
        };

        await _examAttemptRepo.Add(attempt, cancellationToken);
        var saveResult = await _examAttemptRepo.SaveChanges(cancellationToken);
        if (!saveResult)
            return (StudentExamAttemptResult.UnknownError, null);

        return (StudentExamAttemptResult.Success, new CreateExamTokenDto
        {
            StudentId = startExamDto.StudentId,
            ExamAttemptId = attempt.ID,
            MaxDurationInMinutes = exam.MaxDurationInMinutes
        });
    }

    #endregion
}
