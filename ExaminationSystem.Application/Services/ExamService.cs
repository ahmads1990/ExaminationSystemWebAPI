using ExaminationSystem.Application.DTOs;
using ExaminationSystem.Application.DTOs.Exams;
using ExaminationSystem.Application.Interfaces;
using ExaminationSystem.Domain.Entities;
using ExaminationSystem.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ExaminationSystem.Application.Services;

public class ExamService : IExamService
{
    #region Fields

    private readonly IRepository<Exam> _examRepository;
    private readonly IRepository<ExamQuestion> _examQuestionRepository;
    private readonly IRepository<Question> _questionRepository;

    #endregion

    #region Constructor

    public ExamService(IRepository<Exam> examRepository, IRepository<ExamQuestion> examQuestionRepository, IRepository<Question> questionRepository)
    {
        _examRepository = examRepository;
        _examQuestionRepository = examQuestionRepository;
        _questionRepository = questionRepository;
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
                                        .Select(e => new { e.ID, e.ExamStatus, HasQuestions = e.ExamQuestions.Any() })
                                        .FirstOrDefaultAsync(cancellationToken);

        if (exam is null)
            return ExamOperationResult.NotFound;
        if (exam.ExamStatus == ExamStatus.Archived)
            return ExamOperationResult.ExamArchived;
        if (exam.ExamStatus == ExamStatus.Published)
            return ExamOperationResult.AlreadyPublished;
        if (!exam.HasQuestions)
            return ExamOperationResult.NoQuestions;

        var examToUpdate = new Exam
        {
            ID = exam.ID,
            ExamStatus = ExamStatus.Published,
            PublishDate = dto.PublishDate ?? DateTime.UtcNow
        };
        _examRepository.SaveInclude(examToUpdate, nameof(Exam.ExamStatus), nameof(Exam.PublishDate));
        await _examRepository.SaveChanges(cancellationToken);
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

    #region Private Methods

    /// <summary>
    /// Applies search filters to the specified exam query based on the criteria provided in the filter DTO.
    /// </summary>
    /// <param name="query">The initial queryable collection of exams to filter.</param>
    /// <param name="listDto">The search criteria used to filter the exams.</param>
    /// <returns>An IQueryable&lt;Exam&gt; representing the filtered set of exams.</returns>
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
