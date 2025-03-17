using ExaminationSystem.Application.DTOs.Questions;
using ExaminationSystem.Application.Interfaces;
using ExaminationSystem.Domain.Entities;
using ExaminationSystem.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ExaminationSystem.Application.Services;

/// <inheritdoc/>
public class QuestionService : IQuestionService
{
    private readonly IRepository<Question> _questionRepository;
    private readonly IRepository<Choice> _choiceRepository;

    public QuestionService(IRepository<Question> questionRepository, IRepository<Choice> choiceRepository)
    {
        _questionRepository = questionRepository;
        _choiceRepository = choiceRepository;
    }

    /// <inheritdoc/>
    public async Task<(IEnumerable<QuestionDto> Data, int TotalCount)> GetAll(int pageIndex, int pageSize, string? orderBy, SortingDirection sortingDirection, string? body, CancellationToken cancellationToken = default)
    {
        var query = _questionRepository.GetAll();

        if (!string.IsNullOrEmpty(body))
        {
            query = query.Where(q => q.Body.Contains(body));
        }

        // Get count here
        var totalCount = await query.CountAsync();

        Expression<Func<Question, object>> sortingExpression = q => q.CreatedDate;
        if (!string.IsNullOrEmpty(orderBy))
        {
            if (orderBy.Equals("QuestionLevel"))
                sortingExpression = q => q.QuestionLevel;
            else if (orderBy.Equals("Score"))
                sortingExpression = q => q.Score;
            else
                throw new ArgumentException($"Invalid orderBy field: {orderBy}");

        }

        query = sortingDirection == SortingDirection.Ascending ?
                  query.OrderBy(sortingExpression) :
                  query.OrderByDescending(sortingExpression);

        var data = await query.Skip(pageIndex * pageSize).Take(pageSize)
                              .ProjectToType<QuestionDto>()
                              .ToListAsync(cancellationToken);

        return (data, totalCount);
    }

    /// <inheritdoc/>
    public async Task<QuestionDto?> GetByID(int id, CancellationToken cancellationToken = default)
    {
        var question = await _questionRepository.GetByID(id, cancellationToken);

        if (question == null)
            return null;

        return question.Adapt<QuestionDto>();
    }

    /// <inheritdoc/>
    public async Task<QuestionDto> Add(AddQuestionDto questionDto, CancellationToken cancellationToken = default)
    {
        var question = questionDto.Adapt<Question>();

        await _choiceRepository.AddRange(question.Choices, cancellationToken);

        await _questionRepository.Add(question, cancellationToken);
        await SaveChanges();

        return question.Adapt<QuestionDto>();
    }

    /// <inheritdoc/>
    public async Task<QuestionDto> AddRange(ICollection<AddQuestionDto> questionDtos, CancellationToken cancellationToken = default)
    {

    }

    /// <inheritdoc/>
    public async Task<QuestionDto?> Update(UpdateQuestionDto questionDto, CancellationToken cancellationToken = default)
    {
        var question = await _questionRepository.GetByID(questionDto.ID, cancellationToken);

        if (question is null)
            return null;

        question = questionDto.Adapt<Question>();
        question.Choices = questionDto.Choices
                                .Select(c => new Choice
                                {
                                    ID = c.ID,
                                    Body = c.Body,
                                    Question = question
                                }).ToList();

        _questionRepository.Update(question);
        await SaveChanges(cancellationToken);

        return question.Adapt<QuestionDto>();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<int>> Delete(List<int> idsToDelete, CancellationToken cancellationToken = default)
    {
        var questions = await _questionRepository
                .GetByCondition(q => idsToDelete.Contains(q.ID) && !q.ExamQuestions.Any())
                .Include(q => q.Choices)
                .ToListAsync(cancellationToken);

        foreach (var question in questions)
        {
            _choiceRepository.DeleteRange(question.Choices);
        }

        _questionRepository.DeleteRange(questions);
        await SaveChanges(cancellationToken);

        var deletedIds = questions.Select(q => q.ID).ToList();
        return idsToDelete.Except(deletedIds);
    }

    /// <inheritdoc/>
    public async Task SaveChanges(CancellationToken cancellationToken = default)
    {
        await _questionRepository.SaveChanges(cancellationToken);
    }
}
