using ExaminationSystem.Application.DTOs;
using ExaminationSystem.Application.DTOs.Exams;
using ExaminationSystem.Application.DTOs.StudentExams;
using ExaminationSystem.Application.Interfaces;
using ExaminationSystem.Domain.Entities;
using ExaminationSystem.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace ExaminationSystem.Application.Services;

public class ExamService : IExamService
{
    #region Fields

    private readonly IRepository<Exam> _examRepository;
    private readonly IRepository<ExamQuestion> _examQuestionRepository;
    private readonly IRepository<Question> _questionRepository;
    private readonly ILogger<ExamService> _logger;

    #endregion

    #region Constructors

    public ExamService(IRepository<Exam> examRepository, IRepository<ExamQuestion> examQuestionRepository, IRepository<Question> questionRepository, ILogger<ExamService> logger)
    {
        _examRepository = examRepository;
        _examQuestionRepository = examQuestionRepository;
        _questionRepository = questionRepository;
        _logger = logger;
    }

    #endregion

    #region Public Methods

    /// <inheritdoc/>
    public async Task<(IEnumerable<ExamListDto> Data, int TotalCount)> GetAll(ListExamsDto listDto, CancellationToken cancellationToken = default)
    {
        var query = _examRepository.GetAll();
        query = ApplySearchFilters(query, listDto);

        Expression<Func<Exam, object>> sortingExpression = listDto.OrderBy switch
        {
            nameof(Exam.DeadlineDate) => q => q.DeadlineDate,
            _ => q => q.CreatedDate
        };

        query = listDto.SortDirection == SortingDirection.Ascending ?
                  query.OrderBy(sortingExpression) :
                  query.OrderByDescending(sortingExpression);

        var totalCount = await query.CountAsync(cancellationToken);

        var data = await query.Skip(listDto.PageIndex * listDto.PageSize).Take(listDto.PageSize)
                              .ProjectToType<ExamListDto>()
                              .ToListAsync(cancellationToken);

        return (data, totalCount);
    }

    /// <inheritdoc/>
    public async Task<ExamDto?> GetByID(int id, CancellationToken cancellationToken = default)
    {
        return await _examRepository.GetByID(id)
                                    .ProjectToType<ExamDto>()
                                    .FirstOrDefaultAsync();
    }

    /// <inheritdoc/>
    public async Task<(ExamOperationResult Result, int Id)> Add(AddExamDto examDto, CancellationToken cancellationToken = default)
    {
        var exam = examDto.Adapt<Exam>();

        await _examRepository.Add(exam);
        await _examRepository.SaveChanges(cancellationToken);

        return (ExamOperationResult.Success, exam.ID);
    }

    /// <inheritdoc/>
    public async Task<ExamOperationResult> Update(UpdateExamDto examDto, CancellationToken cancellationToken = default)
    {
        var exam = await _examRepository.GetByID(examDto.ID, cancellationToken);

        if (exam is null)
            return ExamOperationResult.NotFound;

        examDto.Adapt(exam);

        _examRepository.Update(exam);
        await SaveChanges(cancellationToken);

        return ExamOperationResult.Success;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<int>> Delete(List<int> idsToDelete, CancellationToken cancellationToken = default)
    {
        var exams = await _examRepository
                     .GetByCondition(q => idsToDelete.Contains(q.ID) && q.ExamStatus != ExamStatus.Published)
                     .Include(q => q.ExamQuestions)
                     .ToListAsync(cancellationToken);

        _examRepository.DeleteRange(exams);
        await SaveChanges(cancellationToken);

        var deletedIds = exams.Select(q => q.ID).ToList();
        return idsToDelete.Except(deletedIds);
    }

    /// <inheritdoc/>
    public async Task SaveChanges(CancellationToken cancellationToken = default)
    {
        await _examRepository.SaveChanges(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ExamOperationResult> Publish(PublishExamDto dto, CancellationToken cancellationToken = default)
    {
        var exam = await _examRepository.GetByCondition(e => e.ID == dto.ID)
                                        .Select(e => new
                                        {
                                            e.ID,
                                            e.ExamStatus,
                                            e.TotalGrade,
                                            HasQuestions = e.ExamQuestions.Any(),
                                            QuestionsScoreSum = e.ExamQuestions.Sum(eq => eq.Question.Score)
                                        })
                                        .FirstOrDefaultAsync(cancellationToken);

        if (exam is null)
        {
            _logger.LogWarning("Cannot publish exam {ExamId}: {Reason}", dto.ID, ExamOperationResult.NotFound);
            return ExamOperationResult.NotFound;
        }
        if (exam.ExamStatus == ExamStatus.Archived)
        {
            _logger.LogWarning("Cannot publish exam {ExamId}: {Reason}", dto.ID, ExamOperationResult.ExamArchived);
            return ExamOperationResult.ExamArchived;
        }
        if (exam.ExamStatus == ExamStatus.Published)
        {
            _logger.LogWarning("Cannot publish exam {ExamId}: {Reason}", dto.ID, ExamOperationResult.AlreadyPublished);
            return ExamOperationResult.AlreadyPublished;
        }
        if (!exam.HasQuestions)
        {
            _logger.LogWarning("Cannot publish exam {ExamId}: {Reason}", dto.ID, ExamOperationResult.NoQuestions);
            return ExamOperationResult.NoQuestions;
        }
        if (exam.QuestionsScoreSum != exam.TotalGrade)
        {
            _logger.LogWarning("Cannot publish exam {ExamId}: {Reason}", dto.ID, ExamOperationResult.ScoresMismatch);
            return ExamOperationResult.ScoresMismatch;
        }

        var examToUpdate = new Exam
        {
            ID = exam.ID,
            ExamStatus = ExamStatus.Published,
            PublishDate = dto.PublishDate ?? DateTime.UtcNow
        };
        _examRepository.SaveInclude(examToUpdate, nameof(Exam.ExamStatus), nameof(Exam.PublishDate));
        await _examRepository.SaveChanges(cancellationToken);
        
        _logger.LogInformation("Exam {ExamId} published", exam.ID);

        return ExamOperationResult.Success;
    }

    /// <inheritdoc />
    public async Task<ExamOperationResult> UnPublish(int id, CancellationToken cancellationToken = default)
    {
        var exam = await _examRepository.GetByCondition(e => e.ID == id)
                                   .Select(e => new { e.ID, e.ExamStatus, HasSubmissions = e.ExamAttempts.Any() })
                                   .FirstOrDefaultAsync(cancellationToken);

        if (exam is null)
            return ExamOperationResult.NotFound;
        if (exam.ExamStatus == ExamStatus.Archived)
            return ExamOperationResult.ExamArchived;
        if (exam.ExamStatus != ExamStatus.Published)
            return ExamOperationResult.AlreadyUnpublished;
        if (exam.HasSubmissions)
            return ExamOperationResult.HasSubmissions;

        var examToUpdate = new Exam
        {
            ID = exam.ID,
            ExamStatus = ExamStatus.Draft,
            PublishDate = null
        };
        _examRepository.SaveInclude(examToUpdate, nameof(Exam.ExamStatus), nameof(Exam.PublishDate));
        await _examRepository.SaveChanges(cancellationToken);
        return ExamOperationResult.Success;
    }

    /// <inheritdoc />
    public async Task<(ExamOperationResult Result, IEnumerable<RejectedEntityDto> Rejected)> AssignQuestions(AssignQuestionsDto dto, CancellationToken cancellationToken = default)
    {
        var exam = await _examRepository.GetByCondition(e => e.ID == dto.ExamId)
                                        .Select(e => new { e.ID, e.ExamStatus })
                                        .FirstOrDefaultAsync(cancellationToken);

        if (exam is null)
            return (ExamOperationResult.NotFound, []);
        if (exam.ExamStatus == ExamStatus.Archived)
            return (ExamOperationResult.ExamArchived, []);
        if (exam.ExamStatus == ExamStatus.Published)
            return (ExamOperationResult.ExamPublished, []);

        // Get existing assignments for this exam
        var assignedQuestionIds = await _examQuestionRepository
            .GetByCondition(eq => eq.ExamId == dto.ExamId)
            .Select(eq => eq.QuestionId)
            .ToListAsync(cancellationToken);

        // Get valid question IDs that actually exist
        var validQuestionIds = await _questionRepository
            .GetByCondition(q => dto.QuestionIds.Contains(q.ID))
            .Select(q => q.ID)
            .ToListAsync(cancellationToken);

        var rejected = new List<RejectedEntityDto>();
        var toAssign = new List<ExamQuestion>();

        foreach (var questionId in dto.QuestionIds)
        {
            if (!validQuestionIds.Contains(questionId))
                rejected.Add(new RejectedEntityDto { Id = questionId, Reason = RejectionReason.NotFound });
            else if (assignedQuestionIds.Contains(questionId))
                rejected.Add(new RejectedEntityDto { Id = questionId, Reason = RejectionReason.AlreadyAssigned });
            else
                toAssign.Add(new ExamQuestion { ExamId = dto.ExamId, QuestionId = questionId, Exam = null!, Question = null! });
        }

        if (toAssign.Any())
        {
            await _examQuestionRepository.AddRange(toAssign, cancellationToken);
            await _examQuestionRepository.SaveChanges(cancellationToken);
            
            _logger.LogInformation("Questions {QuestionIds} assigned to exam {ExamId}", toAssign.Select(q => q.QuestionId), dto.ExamId);
        }

        return (ExamOperationResult.Success, rejected);
    }

    /// <inheritdoc />
    public async Task<(ExamOperationResult Result, IEnumerable<RejectedEntityDto> Rejected)> UnassignQuestions(AssignQuestionsDto dto, CancellationToken cancellationToken = default)
    {
        var exam = await _examRepository.GetByCondition(e => e.ID == dto.ExamId)
                                        .Select(e => new { e.ID, e.ExamStatus, HasSubmissions = e.ExamAttempts.Any() })
                                        .FirstOrDefaultAsync(cancellationToken);

        if (exam is null)
            return (ExamOperationResult.NotFound, []);
        if (exam.ExamStatus == ExamStatus.Archived)
            return (ExamOperationResult.ExamArchived, []);
        if (exam.HasSubmissions)
            return (ExamOperationResult.HasSubmissions, []);

        // Get existing assignments for this exam that match the requested IDs
        var existingAssignments = await _examQuestionRepository
            .GetByCondition(eq => eq.ExamId == dto.ExamId && dto.QuestionIds.Contains(eq.QuestionId))
            .ToListAsync(cancellationToken);

        var assignedQuestionIds = existingAssignments.Select(eq => eq.QuestionId).ToHashSet();

        var rejected = dto.QuestionIds
            .Where(id => !assignedQuestionIds.Contains(id))
            .Select(id => new RejectedEntityDto { Id = id, Reason = RejectionReason.NotAssigned })
            .ToList();

        if (existingAssignments.Any())
        {
            _examQuestionRepository.DeleteRange(existingAssignments);
            await _examQuestionRepository.SaveChanges(cancellationToken);
        }

        return (ExamOperationResult.Success, rejected);
    }

    #endregion

    /// <inheritdoc/>
    public async Task<(ExamOperationResult Result, List<AttemptSummaryDto>? Submissions)> GetExamSubmissions(int examId, int instructorId, CancellationToken cancellationToken = default)
    {
        var exam = await _examRepository.GetByCondition(e => e.ID == examId)
            .Include(e => e.Course)
            .Include(e => e.ExamAttempts)
            .FirstOrDefaultAsync(cancellationToken);

        if (exam is null)
            return (ExamOperationResult.NotFound, null);

        if (exam.Course?.InstructorID != instructorId)
            return (ExamOperationResult.NotOwner, null);

        var submissions = exam.ExamAttempts
            .Where(a => a.ExamAttemptStatus != ExamAttemptStatus.NotStarted && a.ExamAttemptStatus != ExamAttemptStatus.InProgress)
            .Select(a =>
            {
                // We map this here explicitly since Mapster might need the parent Exam reference intact 
                // which is already included in the EF entity, but manual projection is safer for collections inside includes.
                var dto = a.Adapt<AttemptSummaryDto>();
                dto.ExamTitle = exam.Title;
                dto.CourseName = exam.Course?.Title ?? string.Empty;
                dto.ExamType = exam.ExamType;
                dto.MaxGrade = exam.TotalGrade;
                return dto;
            })
            .OrderByDescending(a => a.CreateDate)
            .ToList();

        return (ExamOperationResult.Success, submissions);
    }

    #region Private Methods

    /// <summary>
    /// Applies search filters to the specified exam query based on the criteria provided in the filter DTO.
    /// </summary>
    /// <param name="query">The initial queryable collection of exams to filter.</param>
    /// <param name="listDto">The search criteria used to filter the exams.</param>
    /// <returns>An IQueryable of Exam representing the filtered set of exams.</returns>
    private IQueryable<Exam> ApplySearchFilters(IQueryable<Exam> query, ListExamsDto listDto)
    {
        if (!string.IsNullOrEmpty(listDto.Title))
            query = query.Where(q => q.Title.Contains(listDto.Title));

        if (listDto.ExamType is not null)
            query = query.Where(q => q.ExamType == listDto.ExamType);

        return query;
    }

    #endregion
}
