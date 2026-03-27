using ExaminationSystem.Application.DTOs.Auth;
using ExaminationSystem.Application.DTOs.StudentExams;
using ExaminationSystem.Application.Interfaces;
using ExaminationSystem.Domain.Common;
using ExaminationSystem.Domain.Entities;
using ExaminationSystem.Domain.Interfaces;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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
    private readonly ICurrentUserService _currentUserService;
    private readonly ITenantAccessor _tenantAccessor;
    private readonly ILogger<StudentExamService> _logger;

    #endregion

    #region Constructors

    public StudentExamService(
        IRepository<Exam> examRepo,
        IRepository<Student> studentRepo,
        IRepository<ExamAttempt> examAttemptRepo,
        IRepository<StudentCourses> studentCoursesRepo,
        IRepository<StudentExamsAnswers> answersRepo,
        IAuthService authService,
        IBackgroundJobClient backgroundJobClient,
        ICurrentUserService currentUserService,
        ITenantAccessor tenantAccessor,
        ILogger<StudentExamService> logger)
    {
        _examRepo = examRepo;
        _studentRepo = studentRepo;
        _examAttemptRepo = examAttemptRepo;
        _studentCoursesRepo = studentCoursesRepo;
        _answersRepo = answersRepo;
        _authService = authService;
        _backgroundJobClient = backgroundJobClient;
        _currentUserService = currentUserService;
        _tenantAccessor = tenantAccessor;
        _logger = logger;
    }

    #endregion

    #region Public Methods

    /// <inheritdoc/>
    public async Task<(StudentExamAttemptResult Result, string AccessToken)> StartExamAttempt(StartExamAttemptDto startExamDto, CancellationToken cancellationToken = default)
    {
        var (createResult, examTokenInfoDto) = await CreateExamAttempt(startExamDto, cancellationToken);
        if (createResult != StudentExamAttemptResult.Success || examTokenInfoDto is null)
            return (createResult, string.Empty);

        EnqueueAutoCloseJob(examTokenInfoDto, _tenantAccessor.TenantId);

        var (tokenResult, accessToken) = await _authService.CreateExamAttemptToken(examTokenInfoDto, cancellationToken);
        if (tokenResult != UserOperationResult.Success)
            return (StudentExamAttemptResult.UnknownError, string.Empty);

        _logger.LogInformation("Student {StudentId} started attempt {AttemptId} for exam {ExamId}", startExamDto.StudentId, examTokenInfoDto.ExamAttemptId, startExamDto.ExamId);

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
        var attemptData = await _examAttemptRepo.GetByID(examAttemptId)
            .Select(a => new
            {
                Attempt = a,
                QuestionCount = a.Exam != null ? a.Exam.ExamQuestions.Count : 0
            })
            .FirstOrDefaultAsync(cancellationToken);

        var attempt = attemptData?.Attempt;

        if (attempt is null || attempt.StudentId != studentId)
            return StudentExamAttemptResult.ExamNotFound;

        if (attempt.ExamAttemptStatus != ExamAttemptStatus.InProgress)
            return StudentExamAttemptResult.AttemptAlreadyCompleted;

        attempt.ExamAttemptStatus = ExamAttemptStatus.Completed;
        attempt.EndTime = DateTime.UtcNow;

        _examAttemptRepo.Update(attempt);
        await _examAttemptRepo.SaveChanges(cancellationToken);

        _logger.LogInformation("Student {StudentId} submitted attempt {AttemptId}", studentId, attempt.ID);

        // Enqueue grading job if it has > threshold
        if (attemptData?.QuestionCount > Constants.ImmediateExamGradingThreshold)
        {
            _backgroundJobClient.Enqueue<IGradeExamAttemptJob>(
                job => job.GradeAttemptAsync(attempt.ID, _tenantAccessor.TenantId, CancellationToken.None));
        }

        return StudentExamAttemptResult.Success;
    }

    /// <inheritdoc/>
    public async Task<(StudentExamAttemptResult Result, AttemptResultDto? AttemptResult)> GetAttemptResult(int? examAttemptId, int studentId, CancellationToken cancellationToken = default)
    {
        IQueryable<ExamAttempt> query = _examAttemptRepo.GetAll()
            .Where(a => a.StudentId == studentId)
            .Include(a => a.Exam)
                .ThenInclude(e => e.ExamQuestions);

        ExamAttempt? attempt;
        if (examAttemptId.HasValue)
        {
            attempt = await query.FirstOrDefaultAsync(a => a.ID == examAttemptId.Value, cancellationToken);
        }
        else
        {
            attempt = await query.OrderByDescending(a => a.StartTime).FirstOrDefaultAsync(cancellationToken);
        }

        if (attempt is null)
            return (StudentExamAttemptResult.ExamNotFound, null);

        if (attempt.ExamAttemptStatus == ExamAttemptStatus.InProgress || attempt.ExamAttemptStatus == ExamAttemptStatus.NotStarted)
            return (StudentExamAttemptResult.AttemptNotCompleted, null);

        bool requiresAsyncGrading = attempt.Exam?.ExamQuestions?.Count > Constants.ImmediateExamGradingThreshold;

        if (requiresAsyncGrading && attempt.ExamAttemptStatus != ExamAttemptStatus.Graded)
        {
            return (StudentExamAttemptResult.GradingInProgress, null);
        }

        if (!requiresAsyncGrading && attempt.ExamAttemptStatus != ExamAttemptStatus.Graded)
        {
            // Synchronous grading
            await GradeAttemptAsync(attempt.ID, cancellationToken);
            // Reload attempt
            attempt = await query.FirstOrDefaultAsync(a => a.ID == attempt.ID, cancellationToken);
        }

        if (attempt?.ExamAttemptStatus == ExamAttemptStatus.Graded)
        {
            var resultDto = attempt.Adapt<AttemptResultDto>();
            return (StudentExamAttemptResult.Success, resultDto);
        }

        return (StudentExamAttemptResult.GradingInProgress, null);
    }

    /// <inheritdoc/>
    public async Task<List<AvailableExamDto>> GetAvailableExams(int studentId, CancellationToken cancellationToken = default)
    {
        var enrolledCourseIds = await _studentCoursesRepo.GetAll()
            .Where(sc => sc.StudentID == studentId)
            .Select(sc => sc.CourseID)
            .ToListAsync(cancellationToken);

        if (!enrolledCourseIds.Any())
            return new List<AvailableExamDto>();

        var exams = await _examRepo.GetAll()
            .Include(e => e.Course)
            .Include(e => e.ExamAttempts.Where(a => a.StudentId == studentId))
            .Where(e => enrolledCourseIds.Contains(e.CourseID) &&
                        e.ExamStatus == ExamStatus.Published &&
                        e.DeadlineDate > DateTime.UtcNow)
            .ToListAsync(cancellationToken);

        // Filter out exams where student has exhausted attempts or has an active attempt
        var availableExams = exams.Where(e =>
        {
            var attemptsTaken = e.ExamAttempts.Count;
            var hasActiveAttempt = e.ExamAttempts.Any(a => a.ExamAttemptStatus == ExamAttemptStatus.InProgress);

            return attemptsTaken < e.MaxAttempts && !hasActiveAttempt;
        });

        // Map and populate AttemptsTaken manually
        var result = availableExams.Select(e =>
        {
            var dto = e.Adapt<AvailableExamDto>();
            dto.AttemptsTaken = e.ExamAttempts.Count;
            return dto;
        }).ToList();

        return result;
    }

    /// <inheritdoc/>
    public async Task<List<AttemptSummaryDto>> GetExamHistory(int studentId, CancellationToken cancellationToken = default)
    {
        var attempts = await _examAttemptRepo.GetAll()
            .Where(a => a.StudentId == studentId &&
                        a.ExamAttemptStatus != ExamAttemptStatus.InProgress &&
                        a.ExamAttemptStatus != ExamAttemptStatus.NotStarted)
            .Include(a => a.Exam)
                .ThenInclude(e => e.Course)
            .OrderByDescending(a => a.StartTime)
            .ToListAsync(cancellationToken);

        return attempts.Adapt<List<AttemptSummaryDto>>();
    }

    /// <inheritdoc />
    public async Task GradeAttemptAsync(int attemptId, CancellationToken cancellationToken = default)
    {
        // First check if the attempt exists and hasn't been graded yet
        var attemptStatusCheck = await _examAttemptRepo.GetByID(attemptId)
            .Select(a => new { a.ID, a.ExamAttemptStatus })
            .FirstOrDefaultAsync(cancellationToken);

        if (attemptStatusCheck is null || attemptStatusCheck.ExamAttemptStatus == ExamAttemptStatus.Graded)
            return;

        // Load the full thing
        var attempt = await _examAttemptRepo.GetByID(attemptId)
            .Include(a => a.Answers)
                .ThenInclude(ans => ans.Choice)
            .Include(a => a.Answers)
                .ThenInclude(ans => ans.Question)
            .FirstOrDefaultAsync(cancellationToken);

        if (attempt is null) return;

        // Set status to Grading if it's not already, and it should only happen if it was Completed or TimedOut
        if (attempt.ExamAttemptStatus != ExamAttemptStatus.Grading)
        {
            attempt.ExamAttemptStatus = ExamAttemptStatus.Grading;
            _examAttemptRepo.Update(attempt);
            await _examAttemptRepo.SaveChanges(cancellationToken);
        }

        // Calculate score
        double totalScore = 0;
        if (attempt.Answers != null && attempt.Answers.Any())
        {
            foreach (var answer in attempt.Answers)
            {
                if (answer.Choice != null && answer.Choice.IsCorrect && answer.Question != null)
                {
                    totalScore += answer.Question.Score;
                }
            }
        }

        attempt.Score = totalScore;
        attempt.ExamAttemptStatus = ExamAttemptStatus.Graded;

        _examAttemptRepo.Update(attempt);
        await _examAttemptRepo.SaveChanges(cancellationToken);
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
        {
            _logger.LogWarning("Student {StudentId} blocked from exam attempt {AttemptId}: {Reason}", studentId, examAttemptId, StudentExamAttemptResult.ExamNotFound.ToString());
            return StudentExamAttemptResult.ExamNotFound;
        }

        if (attempt.ExamAttemptStatus != ExamAttemptStatus.InProgress)
        {
            _logger.LogWarning("Student {StudentId} blocked from exam attempt {AttemptId}: {Reason}", studentId, examAttemptId, StudentExamAttemptResult.AttemptAlreadyCompleted.ToString());
            return StudentExamAttemptResult.AttemptAlreadyCompleted;
        }

        return StudentExamAttemptResult.Success;
    }

    /// <summary>
    /// Schedules a Hangfire background job to automatically close the exam attempt.
    /// </summary>
    /// <param name="examTokenInfoDto">The DTO containing attempt information.</param>
    /// <param name="tenantId">Optional tenant ID for job identification.</param>
    private void EnqueueAutoCloseJob(CreateExamTokenDto examTokenInfoDto, int? tenantId = null)
    {
        // Enqueue auto-close job after exam duration expires → sets status to TimedOut
        _backgroundJobClient.Schedule<ICloseExamAttemptJob>(
            job => job.ExecuteAsync(examTokenInfoDto.ExamAttemptId, tenantId, CancellationToken.None),
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
