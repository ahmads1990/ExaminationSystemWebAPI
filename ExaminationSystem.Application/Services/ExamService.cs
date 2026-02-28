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
    private readonly IQuestionService _questionService;

    #endregion

    #region Constructor

    public ExamService(IRepository<Exam> examRepository, IQuestionService questionService)
    {
        _examRepository = examRepository;
        _questionService = questionService;
    }

    #endregion

    #region Public Methods

    /// <inheritdoc/>
    public async Task<(IEnumerable<ExamListDto> Data, int TotalCount)> GetAll(ListExamsDto listDto, CancellationToken cancellationToken = default)
    {
        var query = _examRepository.GetAll();
        query = ApplySearchFilters(query, listDto);

        // Get count here
        var totalCount = await query.CountAsync(cancellationToken);

        Expression<Func<Exam, object>> sortingExpression = q => q.CreatedDate;
        if (!string.IsNullOrEmpty(listDto.OrderBy))
        {
            if (listDto.OrderBy.Equals(nameof(Exam.DeadlineDate)))
                sortingExpression = q => q.DeadlineDate;
            else
                throw new ArgumentException($"Invalid orderBy field: {listDto.OrderBy}");
        }

        query = listDto.SortDirection == SortingDirection.Ascending ?
                  query.OrderBy(sortingExpression) :
                  query.OrderByDescending(sortingExpression);

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
