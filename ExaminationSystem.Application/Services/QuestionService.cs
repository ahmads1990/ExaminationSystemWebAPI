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

    #region Publich Methods

    /// <inheritdoc/>
    public async Task<(IEnumerable<QuestionDto> Data, int TotalCount)> GetAll(ListQuestionsDto listDto, CancellationToken cancellationToken = default)
    {
        var query = _questionRepository.GetAll();
        query = ApplySearchFilters(query, listDto);

        Expression<Func<Question, object>> sortingExpression = listDto.OrderBy switch
        {
            nameof(Question.QuestionLevel) => q => q.QuestionLevel,
            nameof(Question.Score) => q => q.Score,
            nameof(Question.ID) => q => q.ID,
            _ => q => q.CreatedDate
        };

        query = listDto.SortDirection == SortingDirection.Ascending
                    ? query.OrderBy(sortingExpression)
                    : query.OrderByDescending(sortingExpression);

        var totalCount = await query.CountAsync(cancellationToken);

        var data = await query.Skip(listDto.PageIndex * listDto.PageSize).Take(listDto.PageSize)
                              .ProjectToType<QuestionDto>()
                              .ToListAsync(cancellationToken);

        return (data, totalCount);
    }

    /// <inheritdoc/>
    public async Task<QuestionDto?> GetByID(int id, CancellationToken cancellationToken = default)
    {
        return await _questionRepository.GetByID(id)
                                        .ProjectToType<QuestionDto>()
                                        .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<QuestionDto> Add(AddQuestionDto questionDto, CancellationToken cancellationToken = default)
    {
        var question = questionDto.Adapt<Question>();

        await _questionRepository.Add(question, cancellationToken);
        await SaveChanges(cancellationToken);

        return question.Adapt<QuestionDto>();
    }

    /// <inheritdoc/>
    public async Task<QuestionDto?> Update(UpdateQuestionDto questionDto, CancellationToken cancellationToken = default)
    {
        var question = await _questionRepository.GetByID(questionDto.ID)
            .Include(q => q.Choices)
            .FirstOrDefaultAsync(cancellationToken);

        if (question is null)
            return null;

        // Map scalar properties onto the tracked entity
        question.Body = questionDto.Body;
        question.Score = questionDto.Score;
        question.QuestionLevel = questionDto.QuestionLevel;

        // Build a lookup of incoming choice IDs
        var incomingIds = questionDto.Choices
            .Where(c => c.ID > 0)
            .Select(c => c.ID)
            .ToHashSet();

        // Remove choices that are no longer in the DTO
        var choicesToRemove = question.Choices
            .Where(c => !incomingIds.Contains(c.ID))
            .ToList();

        foreach (var choice in choicesToRemove)
        {
            question.Choices.Remove(choice);
            _choiceRepository.Delete(choice);
        }

        // Update existing + add new
        foreach (var choiceDto in questionDto.Choices)
        {
            if (choiceDto.ID > 0)
            {
                // Update existing choice
                var existing = question.Choices.FirstOrDefault(c => c.ID == choiceDto.ID);
                if (existing is not null)
                {
                    existing.Body = choiceDto.Body;
                    existing.IsCorrect = choiceDto.IsCorrect;
                }
            }
            else
            {
                // Add new choice
                question.Choices.Add(new Choice
                {
                    Body = choiceDto.Body,
                    IsCorrect = choiceDto.IsCorrect,
                    QuestionId = question.ID
                });
            }
        }

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

    #endregion

    #region Private Methods

    /// <summary>
    /// Applies search filters to a collection of questions based on the specified criteria.
    /// </summary>
    /// <remarks>Filters are applied for exam association and question body text. The returned query can be
    /// further composed or executed as needed.</remarks>
    /// <param name="query">The queryable collection of questions to filter.</param>
    /// <param name="listDto">The search criteria used to filter the questions. Cannot be null.</param>
    /// <returns>An IQueryable<Question> containing questions that match the specified search filters.</returns>
    private IQueryable<Question> ApplySearchFilters(IQueryable<Question> query, ListQuestionsDto listDto)
    {
        if (listDto.ExamID.HasValue)
        {
            query = query.Where(q => q.ExamQuestions.Any(eq => eq.ExamId == listDto.ExamID.Value));
        }

        if (!string.IsNullOrEmpty(listDto.Body))
        {
            query = query.Where(q => q.Body.Contains(listDto.Body));
        }
        return query;
    }

    #endregion
}
