using ExaminationSystem.Application.DTOs.Exams;
using ExaminationSystem.Application.Interfaces;
using ExaminationSystem.Domain.Entities;
using ExaminationSystem.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ExaminationSystem.Application.Services;

public class ExamService : IExamService
{
    private readonly IRepository<Exam> _examRepository;
    private readonly IQuestionService _questionService;

    public ExamService(IRepository<Exam> examRepository, IQuestionService questionService)
    {
        _examRepository = examRepository;
        _questionService = questionService;
    }
    public async Task<(IEnumerable<ExamDto> Data, int TotalCount)> GetAll(int pageIndex, int pageSize, string? orderBy, SortingDirection sortingDirection, string? body, CancellationToken cancellationToken = default)
    {
        var query = _examRepository.GetAll();

        //if (!string.IsNullOrEmpty(body))
        //{
        //    query = query.Where(q => q.Body.Contains(body));
        //}

        // Get count here
        var totalCount = await query.CountAsync();

        Expression<Func<Exam, object>> sortingExpression = q => q.CreatedDate;
        //if (!string.IsNullOrEmpty(orderBy))
        //{
        //    if (orderBy.Equals("QuestionLevel"))
        //        sortingExpression = q => q.QuestionLevel;
        //    else if (orderBy.Equals("Score"))
        //        sortingExpression = q => q.Score;
        //    else
        //        throw new ArgumentException($"Invalid orderBy field: {orderBy}");

        //}

        query = sortingDirection == SortingDirection.Ascending ?
                  query.OrderBy(sortingExpression) :
                  query.OrderByDescending(sortingExpression);

        var data = await query.Skip(pageIndex * pageSize).Take(pageSize)
                              .ProjectToType<ExamDto>()
                              .ToListAsync(cancellationToken);

        return (data, totalCount);
    }

    public async Task<ExamDto?> GetByID(int id, CancellationToken cancellationToken = default)
    {
        var exam = await _examRepository.GetByID(id, cancellationToken);

        if (exam == null)
            return null;

        return exam.Adapt<ExamDto>();
    }

    public async Task<ExamDto> Add(AddExamDto examDto, CancellationToken cancellationToken = default)
    {
        var exam = examDto.Adapt<Exam>();
        // Temp for now
        exam.CourseID = 1;

        await _examRepository.Add(exam);
        await _examRepository.SaveChanges(cancellationToken);

        return exam.Adapt<ExamDto>();
    }

    public async Task<ExamDto?> Update(UpdateExamDto examDto, CancellationToken cancellationToken = default)
    {
        var exam = await _examRepository.GetByID(examDto.ID, cancellationToken);

        if (exam is null)
            return null;

        exam = examDto.Adapt<Exam>();

        _examRepository.Update(exam);
        await SaveChanges(cancellationToken);

        return exam.Adapt<ExamDto>();
    }

    public async Task<IEnumerable<int>> Delete(List<int> idsToDelete, CancellationToken cancellationToken = default)
    {
        var exams = await _examRepository
                     .GetByCondition(q => idsToDelete.Contains(q.ID) && !q.IsPublished)
                     .Include(q => q.ExamQuestions)
                     .ToListAsync(cancellationToken);

        //foreach (var exam in exams)
        //{
        //    _questionService.DeleteRange(exam.ExamQuestions);
        //}

        _examRepository.DeleteRange(exams);
        await SaveChanges(cancellationToken);

        var deletedIds = exams.Select(q => q.ID).ToList();
        return idsToDelete.Except(deletedIds);
    }

    public async Task SaveChanges(CancellationToken cancellationToken = default)
    {
        await _examRepository.SaveChanges(cancellationToken);
    }
}

