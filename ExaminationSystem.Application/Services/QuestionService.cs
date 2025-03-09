using ExaminationSystem.Application.DTOs.Questions;
using ExaminationSystem.Application.Interfaces;
using ExaminationSystem.Domain.Entities;
using ExaminationSystem.Domain.Interfaces;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
namespace ExaminationSystem.Application.Services;

public class QuestionService : IQuestionService
{
    private readonly IRepository<Question> _questionRepository;
    private readonly IRepository<Choice> _choiceRepository;
    private readonly IMapper _mapper;

    public QuestionService(IRepository<Question> questionRepository, IRepository<Choice> choiceRepository, IMapper mapper)
    {
        _questionRepository = questionRepository;
        _choiceRepository = choiceRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<QuestionDto>> GetAll(int start, int length)
    {
        var query = _questionRepository.GetAll()
                 .ProjectToType<QuestionDto>()
                 .Skip(start * length)
                 .Take(length);

        return await query.ToListAsync();
    }

    public async Task<QuestionDto?> GetByID(int id)
    {
        var question = await _questionRepository.GetByID(id);

        if (question == null)
            return null;

        return _mapper.Map<QuestionDto>(question);
    }

    public async Task<QuestionDto> Add(AddQuestionDto questionDto)
    {
        var question = _mapper.Map<Question>(questionDto);

        await _choiceRepository.AddRange(question.Choices);

        await _questionRepository.Add(question);
        await SaveChanges();

        return _mapper.Map<QuestionDto>(question);
    }

    public async Task<QuestionDto?> Update(UpdateQuestionDto questionDto)
    {
        var question = await _questionRepository.GetByID(questionDto.ID);

        if (question is null)
            return null;

        question.Body = questionDto.Body;
        question.Score = questionDto.Score;
        question.QuestionLevel = (QuestionLevel)questionDto.QuestionLevel;

        question.Choices = questionDto.Choices
                                .Select(c => new Choice { ID = c.ID, Body = c.Body, Question = question })
                                .ToList();

        _questionRepository.Update(question);
        await SaveChanges();

        return _mapper.Map<QuestionDto>(question);
    }

    public async Task<IEnumerable<int>> Delete(List<int> idsToDelete)
    {
        var questions = await _questionRepository
                .GetByCondition(q => idsToDelete.Contains(q.ID) && !q.ExamQuestions.Any())
                .Include(q => q.Choices)
                .ToListAsync();

        foreach (var question in questions)
        {
            _choiceRepository.DeleteRange(question.Choices);
        }

        _questionRepository.DeleteRange(questions);
        await SaveChanges();

        var deletedIds = questions.Select(q => q.ID).ToList();
        return idsToDelete.Except(deletedIds);
    }

    public async Task SaveChanges()
    {
        await _questionRepository.SaveChanges();
    }
}
