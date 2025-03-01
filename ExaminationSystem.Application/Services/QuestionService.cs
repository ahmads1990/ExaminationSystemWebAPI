using ExaminationSystem.Application.DTOs.Questions;
using ExaminationSystem.Application.Interfaces;
using ExaminationSystem.Domain.Entities;
using ExaminationSystem.Domain.Interfaces;
using MapsterMapper;
namespace ExaminationSystem.Application.Services;

public class QuestionService : IQuestionService
{
    private readonly IChoiceService _choiceService;
    private readonly IRepository<Question> _repository;
    private readonly IMapper _mapper;

    public QuestionService(IRepository<Question> repository, IMapper mapper, IChoiceService choiceService)
    {
        _repository = repository;
        _mapper = mapper;
        _choiceService = choiceService;
    }

    public IQueryable<Question> GetAll()
    {
        return _repository.GetAll();
    }

    public async Task<QuestionDto> Add(AddQuestionDto questionDto)
    {
        var question = _mapper.Map<Question>(questionDto);

        question.Choices = await _choiceService.AddRange(questionDto.Choices);
        question.Answer = question.Choices.ElementAt(questionDto.AnswerOrder);

        await _repository.Add(question);
        await SaveChanges();

        return _mapper.Map<QuestionDto>(questionDto);
    }

    //public Task<Question?> GetByID(int id)
    //{
    //    throw new NotImplementedException();
    //}

    //public Task<IEnumerable<Question>> AddRange(AddQuestionsRequest request)
    //{
    //    throw new NotImplementedException();
    //}

    //public void UpdateQuestion(UpdateQuestionRequest request)
    //{
    //    throw new NotImplementedException();
    //}

    //public void Delete(DeleteQuestionsRequest request)
    //{
    //    throw new NotImplementedException();
    //}

    public async Task SaveChanges()
    {
        await _repository.SaveChanges();
    }
}
